using CommerceHub.Domain.Entities;
using CommerceHub.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CommerceHub.Infrastructure.Persistence;

public class MongoContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoContext(IMongoClient mongoClient, IOptions<MongoDbSettings> options)
    {
        _settings = options.Value;
        _database = mongoClient.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<Product> Products =>
        _database.GetCollection<Product>(_settings.ProductsCollectionName);

    public IMongoCollection<Order> Orders =>
        _database.GetCollection<Order>(_settings.OrdersCollectionName);
}