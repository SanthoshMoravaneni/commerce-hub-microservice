using CommerceHub.Application.Interfaces;
using CommerceHub.Domain.Entities;
using MongoDB.Driver;

namespace CommerceHub.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly MongoContext _context;

    public OrderRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.InsertOneAsync(order, cancellationToken: cancellationToken);
        return order;
    }

    public async Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
        return await _context.Orders.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Order> ReplaceAsync(Order order, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Id, order.Id);

        await _context.Orders.ReplaceOneAsync(
            filter,
            order,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);

        return order;
    }
}