using CommerceHub.Domain.Entities;

namespace CommerceHub.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Product>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
    Task<bool> TryDecrementStockAsync(string productId, int quantity, CancellationToken cancellationToken = default);
    Task<Product?> AdjustStockAsync(string productId, int delta, CancellationToken cancellationToken = default);
}