using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GeekShopping.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public CartController(IProductService productService,
        ICartService cartService,
        ICouponService couponService)
        {
            _productService = productService;
            _cartService = cartService;
            _couponService = couponService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await FindUserCart());
        }

        [HttpPost]
        [ActionName("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon(CartViewModel cart)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault().Value;

            var response = await _cartService.ApplyCoupon(cart, accessToken);

            if (response)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        [ActionName("RemoveCoupon")]
        public async Task<IActionResult> RemoveCoupon()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault().Value;

            var response = await _cartService.RemoveCoupon(userId, accessToken);

            if (response)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            return View(await FindUserCart());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CartViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.Checkout(model.CartHeader, accessToken);

            if (response != null)
            {
                return RedirectToAction(nameof(Confirmation));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {
            return View();
        }
        public async Task<IActionResult> Remove(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault().Value;

            var response = await _cartService.RemoveFromCart(id, accessToken);

            if (response)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        private async Task<CartViewModel> FindUserCart()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault().Value;

            var response = await _cartService.FindCartByUserId(userId, accessToken);
            if (response?.CartHeader != null)
            {
                if (!string.IsNullOrEmpty(response.CartHeader.CouponCode))
                {
                    var coupon = await _couponService.GetCoupon(response.CartHeader.CouponCode,accessToken);
                    if (coupon?.CouponCode != null)
                    {
                        response.CartHeader.DiscountTotal = coupon.DiscountAmount;
                    }
                }
                response.CartHeader.PurchaseAmount = 0;
                foreach (var detail in response.CartDetails)
                {
                    response.CartHeader.PurchaseAmount += (detail.Product.Price * detail.Count);

                }
                Console.WriteLine(response.CartHeader.PurchaseAmount);
                Console.WriteLine(response.CartHeader.DiscountTotal);
                response.CartHeader.PurchaseAmount -= response.CartHeader.DiscountTotal;
                Console.WriteLine(response.CartHeader.PurchaseAmount);
            }
            return response;
        }
    }
}