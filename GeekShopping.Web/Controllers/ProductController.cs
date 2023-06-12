using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductIndex()
        {
            var products = await _productService.FindAllProducts();
            return View(products);
        }

        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.CreateProduct(model);
                if (response != null) return RedirectToAction(nameof(ProductIndex));
            }
            return View(model);
        }

        public async Task<IActionResult> ProductUpdate(int Id)
        {
            var product = await _productService.FindProductById(Id);
            if (product != null) return View(product);
            return NotFound();

        }
        [HttpPost]
        public async Task<IActionResult> ProductUpdate(ProductModel model)
        {
            Console.WriteLine("entrou");
            var response = await _productService.UpdateProduct(model);
            if (response != null) return RedirectToAction(nameof(ProductIndex));

            return View(model);
        }

        public async Task<IActionResult> ProductDelete(int Id)
        {
            var product = await _productService.FindProductById(Id);
            if (product != null) return View(product);
            return NotFound();

        }
        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductModel model)
        {
            var response = await _productService.DeleteProductById(model.Id);
            if (response) return RedirectToAction(nameof(ProductIndex));
            return View(model);
        }
    }
}