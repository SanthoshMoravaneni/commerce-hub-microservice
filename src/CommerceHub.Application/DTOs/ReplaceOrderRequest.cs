using CommerceHub.Domain.Enums;

namespace CommerceHub.Application.DTOs;

public class ReplaceOrderRequest
{
    public List<ReplaceOrderItemRequest> Items { get; set; } = new();
    public OrderStatus Status { get; set; }
}

public class ReplaceOrderItemRequest
{
    public string ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}