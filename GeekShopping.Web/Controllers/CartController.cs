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

        public CartController(IProductService productService,
        ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }
        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await findUserCart());
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

        private async Task<CartViewModel> findUserCart()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault().Value;

            var response = await _cartService.FindCartByUserId(userId, accessToken);
            if (response?.CartHeader != null)
            {
                response.CartHeader.PurchaseAmount = 0;
                foreach (var detail in response.CartDetails)
                {
                    response.CartHeader.PurchaseAmount += (detail.Product.Price * detail.Count);
                    Console.WriteLine(response.CartHeader.PurchaseAmount);
                    Console.WriteLine(detail.Product.Price);
                    Console.WriteLine(detail.Count);
                }
            }
            return response;
        }
    }
}