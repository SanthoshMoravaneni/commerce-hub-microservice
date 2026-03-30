using CommerceHub.Domain.Entities;

namespace CommerceHub.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Order> ReplaceAsync(Order order, CancellationToken cancellationToken = default);
}