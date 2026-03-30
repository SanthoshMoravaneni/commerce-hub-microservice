namespace CommerceHub.Domain.Events;

public class OrderCreatedEvent
{
    public string OrderId { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}