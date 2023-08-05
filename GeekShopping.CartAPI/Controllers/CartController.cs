using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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


    [HttpGet("find-cart/{id}")]
    public async Task<ActionResult<CartVO>> FindById(string id)
    {
        var cart = await _cartRepository.FindCartByUserId(id);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPost("add-cart")]
    public async Task<ActionResult<CartVO>> AddCart(CartVO cartVO)
    {
        var cart = await _cartRepository.SaveOrUpdateCart(cartVO);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPut("update-cart")]
    public async Task<ActionResult<CartVO>> UpdateCart(CartVO cartVO)
    {
        var cart = await _cartRepository.SaveOrUpdateCart(cartVO);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpDelete("remove-cart/{id}")]
    public async Task<ActionResult<CartVO>> RemoveCart(int id)
    {
        var status = await _cartRepository.RemoveFromCart(id);
        if (!status) return BadRequest();
        return Ok(status);
    }

    [HttpPost("apply-coupon")]
    public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO cartVO)
    {
        var status = await _cartRepository.ApplyCoupon(cartVO.CartHeader.UserId, cartVO.CartHeader.CouponCode);
        if (!status) return NotFound();
        return Ok(status);
    }

    [HttpDelete("remove-coupon/{userId}")]
    public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
    {
        var status = await _cartRepository.RemoveCoupon(userId);
        if (!status) return NotFound();
        return Ok(status);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO vo)
    {
        var token = await HttpContext.GetTokenAsync("access_token");
        if(vo?.UserId == null) return BadRequest();
        var cart = await _cartRepository.FindCartByUserId(vo.UserId);
        if (cart == null) return NotFound();
        if (!string.IsNullOrEmpty(vo.CouponCode))
        {
            CouponVO coupon = await _couponRepository.GetCouponByCouponCode(vo.CouponCode, token);
            if (vo.DiscountTotal != coupon.DiscountAmount)
            {
                return StatusCode(412);
            }
        }
        vo.CartDetails = cart.CartDetails;
        vo.DateTime = DateTime.Now;

        // Calling rabbitMQ
        _rabbitMQMessageSender.SendMessage(vo, "checkoutqueue");
        return Ok(vo);
    }
}
