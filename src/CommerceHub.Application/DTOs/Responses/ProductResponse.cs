namespace CommerceHub.Application.DTOs.Responses;

public class ProductResponse
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Stock { get; set; }
    public decimal Price { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}