using CommerceHub.Application.Interfaces;
using CommerceHub.Domain.Entities;
using MongoDB.Driver;

namespace CommerceHub.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly MongoContext _context;

    public ProductRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
        return await _context.Products.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        var filter = Builders<Product>.Filter.In(x => x.Id, idList);
        return await _context.Products.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<bool> TryDecrementStockAsync(string productId, int quantity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Eq(x => x.Id, productId),
            Builders<Product>.Filter.Gte(x => x.Stock, quantity));

        var update = Builders<Product>.Update
            .Inc(x => x.Stock, -quantity)
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        var result = await _context.Products.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<Product?> AdjustStockAsync(string productId, int delta, CancellationToken cancellationToken = default)
    {
        var filter = delta < 0
            ? Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(x => x.Id, productId),
                Builders<Product>.Filter.Gte(x => x.Stock, Math.Abs(delta)))
            : Builders<Product>.Filter.Eq(x => x.Id, productId);

        var update = Builders<Product>.Update
            .Inc(x => x.Stock, delta)
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        return await _context.Products.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<Product>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);
    }
}