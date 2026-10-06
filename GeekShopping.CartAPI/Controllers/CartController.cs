using GeekShopping.CartAPI.Data;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GeekShopping.CartAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize("ApiScope")]
public class CartController : ControllerBase
{
    private ICartRepository _cartRepository;
    private ICouponRepository _couponRepository;
    private IRabbitMQMessageSender _rabbitMQMessageSender;
    public CartController(ICartRepository cartRepository,
     IRabbitMQMessageSender rabbitMQMessageSender,
     ICouponRepository couponRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _rabbitMQMessageSender = rabbitMQMessageSender ?? throw new ArgumentNullException(nameof(rabbitMQMessageSender));
        _couponRepository = couponRepository ?? throw new ArgumentNullException(nameof(couponRepository));
    }


    // The IdentityServer subject; JwtBearer maps "sub" to NameIdentifier unless claim mapping is off
    private string? CurrentUserId => User?.FindFirst("sub")?.Value ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // A user may only read or change their own cart
    private bool IsCurrentUser(string? userId) => userId != null && userId == CurrentUserId;

    [HttpGet("find-cart/{id}")]
    public async Task<ActionResult<CartVO>> FindById(string id)
    {
        if (!IsCurrentUser(id)) return Forbid();
        var cart = await _cartRepository.FindCartByUserId(id);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPost("add-cart")]
    public async Task<ActionResult<CartVO>> AddCart(CartVO cartVO)
    {
        if (cartVO.CartHeader == null || cartVO.CartDetails?.Any() != true) return BadRequest("The cart needs a header and at least one item.");
        if (!IsCurrentUser(cartVO.CartHeader.UserId)) return Forbid();
        var cart = await _cartRepository.SaveOrUpdateCart(cartVO);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPut("update-cart")]
    public async Task<ActionResult<CartVO>> UpdateCart(CartVO cartVO)
    {
        if (cartVO.CartHeader == null || cartVO.CartDetails?.Any() != true) return BadRequest("The cart needs a header and at least one item.");
        if (!IsCurrentUser(cartVO.CartHeader.UserId)) return Forbid();
        var cart = await _cartRepository.SaveOrUpdateCart(cartVO);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpDelete("remove-cart/{id}")]
    public async Task<ActionResult<CartVO>> RemoveCart(int id)
    {
        var cart = await _cartRepository.FindCartByUserId(CurrentUserId!);
        // An item from someone else's cart looks the same as one that does not exist
        if (cart?.CartDetails?.Any(d => d.Id == id) != true) return NotFound();
        var status = await _cartRepository.RemoveFromCart(id);
        if (!status) return BadRequest();
        return Ok(status);
    }

    [HttpPost("apply-coupon")]
    public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO cartVO)
    {
        if (cartVO.CartHeader == null) return BadRequest();
        if (!IsCurrentUser(cartVO.CartHeader.UserId)) return Forbid();
        var status = await _cartRepository.ApplyCoupon(cartVO.CartHeader.UserId, cartVO.CartHeader.CouponCode);
        if (!status) return NotFound();
        return Ok(status);
    }

    [HttpDelete("remove-coupon/{userId}")]
    public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
    {
        if (!IsCurrentUser(userId)) return Forbid();
        var status = await _cartRepository.RemoveCoupon(userId);
        if (!status) return NotFound();
        return Ok(status);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO vo)
    {
        var token = await HttpContext.GetTokenAsync("access_token");
        if(vo?.UserId == null) return BadRequest();
        if (!IsCurrentUser(vo.UserId)) return Forbid();
        var cart = await _cartRepository.FindCartByUserId(vo.UserId);
        // FindCartByUserId returns an empty cart, not null, for a user without one
        if (cart?.CartDetails == null || !cart.CartDetails.Any()) return BadRequest("The cart is empty.");
        decimal discount = 0;
        if (!string.IsNullOrEmpty(vo.CouponCode))
        {
            CouponVO coupon = await _couponRepository.GetCouponByCouponCode(vo.CouponCode, token);
            // The coupon repository answers an unknown code with an empty coupon
            if (coupon?.CouponCode == null || vo.DiscountTotal != coupon.DiscountAmount)
            {
                return StatusCode(412);
            }
            discount = coupon.DiscountAmount ?? 0;
        }
        vo.CartDetails = cart.CartDetails;
        // OrderAPI charges PurchaseAmount, so it comes from the saved cart, not from the request
        vo.DiscountTotal = discount;
        vo.PurchaseAmount = PurchaseAmountCalculator.Calculate(cart.CartDetails, discount);
        vo.DateTime = DateTime.Now;

        // Calling rabbitMQ
        _rabbitMQMessageSender.SendMessage(vo, "checkoutqueue");
        await _cartRepository.ClearCart(vo.UserId);
        return Ok(vo);
    }
}
