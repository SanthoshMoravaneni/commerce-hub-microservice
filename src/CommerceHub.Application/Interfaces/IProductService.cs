using CommerceHub.Application.DTOs;
using CommerceHub.Application.DTOs.Responses;

namespace CommerceHub.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> AdjustStockAsync(string productId, AdjustStockRequest request, CancellationToken cancellationToken = default);
}