using CommerceHub.Application.DTOs;
using CommerceHub.Application.DTOs.Responses;

namespace CommerceHub.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CheckoutAsync(CheckoutOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<OrderResponse> ReplaceAsync(string id, ReplaceOrderRequest request, CancellationToken cancellationToken = default);
}