using GeekShopping.CouponAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CouponAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CouponController : ControllerBase
{
    private readonly ILogger<CouponController> _logger;

    public CouponController(ILogger<CouponController> logger)
    {
        _logger = logger;
    }

    // [HttpGet(Name = "GetCoupon")]
    // public IEnumerable<Coupon> Get()
    // {
    //     return Enumerable.Range(1, 5).Select(index => new Coupon
    //     {
    //         Date = DateTime.Now.AddDays(index),
    //         TemperatureC = Random.Shared.Next(-20, 55),
    //         Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //     })
    //     .ToArray();
    // }
}
