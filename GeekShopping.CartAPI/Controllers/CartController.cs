using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController : ControllerBase
{
    private ICartRepository _repository;
    public CartController(ICartRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }


    [HttpGet("find-cart/{id}")]
    public async Task<ActionResult<CartVO>> FindById(string userId)
    {
        var cart = await _repository.FindCartByUserId(userId);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPost("add-cart/{id}")]
    public async Task<ActionResult<CartVO>> AddCart(CartVO cartVO)
    {
        var cart = await _repository.SaveOrUpdateCart(cartVO);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPut("update-cart/{id}")]
    public async Task<ActionResult<CartVO>> UpdateCart(CartVO cartVO)
    {
        var cart = await _repository.SaveOrUpdateCart(cartVO);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpDelete("remove-cart/{id}")]
    public async Task<ActionResult<CartVO>> RemoveCart(long id)
    {
        var status = await _repository.RemoveFromCart(id);
        if (!status) return BadRequest();
        return Ok(status);
    }
}
