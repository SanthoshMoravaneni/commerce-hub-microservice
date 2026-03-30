using CommerceHub.Application.Common;
using CommerceHub.Application.DTOs;
using CommerceHub.Application.DTOs.Responses;
using CommerceHub.Application.Interfaces;

namespace CommerceHub.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponse> AdjustStockAsync(
        string productId,
        AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ValidationException("Product id is required.");
        }

        if (request is null)
        {
            throw new ValidationException("Request body is required.");
        }

        if (request.Delta == 0)
        {
            throw new ValidationException("Stock delta cannot be zero.");
        }

        var existingProduct = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (existingProduct is null)
        {
            throw new NotFoundException($"Product with id '{productId}' was not found.");
        }

        var updatedProduct = await _productRepository.AdjustStockAsync(productId, request.Delta, cancellationToken);

        if (updatedProduct is null)
        {
            throw new ConflictException("Stock adjustment failed because it would result in negative stock.");
        }

        return new ProductResponse
        {
            Id = updatedProduct.Id,
            Name = updatedProduct.Name,
            Stock = updatedProduct.Stock,
            Price = updatedProduct.Price,
            UpdatedAtUtc = updatedProduct.UpdatedAtUtc
        };
    }
}