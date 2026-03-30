namespace CommerceHub.Application.DTOs;

public class CheckoutOrderRequest
{
    public List<CheckoutOrderItemRequest> Items { get; set; } = new();
}

public class CheckoutOrderItemRequest
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
}