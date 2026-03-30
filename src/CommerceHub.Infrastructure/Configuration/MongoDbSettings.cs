namespace CommerceHub.Infrastructure.Configuration;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
    public string ProductsCollectionName { get; set; } = "Products";
    public string OrdersCollectionName { get; set; } = "Orders";
}