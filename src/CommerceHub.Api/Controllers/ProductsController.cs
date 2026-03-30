using CommerceHub.Application.DTOs;
using CommerceHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommerceHub.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPatch("{id}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AdjustStock(
        string id,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.AdjustStockAsync(id, request, cancellationToken);
        return Ok(result);
    }
}