using CommerceHub.Domain.Enums;

namespace CommerceHub.Application.DTOs.Responses;

public class OrderResponse
{
    public string Id { get; set; } = default!;
    public List<OrderItemResponse> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class OrderItemResponse
{
    public string ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}