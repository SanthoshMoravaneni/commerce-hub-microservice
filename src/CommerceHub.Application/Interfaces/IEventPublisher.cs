using CommerceHub.Domain.Events;

namespace CommerceHub.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken = default);
}