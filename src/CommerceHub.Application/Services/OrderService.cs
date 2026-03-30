using CommerceHub.Application.Common;
using CommerceHub.Application.DTOs;
using CommerceHub.Application.DTOs.Responses;
using CommerceHub.Application.Interfaces;
using CommerceHub.Domain.Entities;
using CommerceHub.Domain.Enums;
using CommerceHub.Domain.Events;

namespace CommerceHub.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrderResponse> CheckoutAsync(
        CheckoutOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Request body is required.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException("Order must contain at least one item.");
        }

        if (request.Items.Any(x => string.IsNullOrWhiteSpace(x.ProductId)))
        {
            throw new ValidationException("Each item must have a product id.");
        }

        if (request.Items.Any(x => x.Quantity <= 0))
        {
            throw new ValidationException("Each item quantity must be greater than zero.");
        }

        var groupedItems = request.Items
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToList();

        var productIds = groupedItems.Select(x => x.ProductId).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new NotFoundException("One or more products were not found.");
        }

        var productMap = products.ToDictionary(x => x.Id, x => x);

        foreach (var item in groupedItems)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
            {
                throw new NotFoundException($"Product with id '{item.ProductId}' was not found.");
            }

            if (product.Stock < item.Quantity)
            {
                throw new ConflictException($"Insufficient stock for product '{product.Name}'.");
            }
        }

        var decrementedItems = new List<(string ProductId, int Quantity)>();

        try
        {
            foreach (var item in groupedItems)
            {
                var success = await _productRepository.TryDecrementStockAsync(
                    item.ProductId,
                    item.Quantity,
                    cancellationToken);

                if (!success)
                {
                    throw new ConflictException($"Insufficient stock for product '{productMap[item.ProductId].Name}'.");
                }

                decrementedItems.Add((item.ProductId, item.Quantity));
            }

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                Status = OrderStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Items = groupedItems.Select(item =>
                {
                    var product = productMap[item.ProductId];
                    return new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(x => x.UnitPrice * x.Quantity);

            var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);

            await _eventPublisher.PublishOrderCreatedAsync(
                new OrderCreatedEvent
                {
                    OrderId = createdOrder.Id,
                    TotalAmount = createdOrder.TotalAmount,
                    CreatedAtUtc = createdOrder.CreatedAtUtc
                },
                cancellationToken);

            return MapToResponse(createdOrder);
        }
        catch
        {
            foreach (var item in decrementedItems)
            {
                await _productRepository.AdjustStockAsync(item.ProductId, item.Quantity, cancellationToken);
            }

            throw;
        }
    }

    public async Task<OrderResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<OrderResponse> ReplaceAsync(
        string id,
        ReplaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException("Order id is required.");
        }

        if (request is null)
        {
            throw new ValidationException("Request body is required.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException("Order must contain at least one item.");
        }

        if (request.Items.Any(x => string.IsNullOrWhiteSpace(x.ProductId)))
        {
            throw new ValidationException("Each item must have a product id.");
        }

        if (request.Items.Any(x => string.IsNullOrWhiteSpace(x.ProductName)))
        {
            throw new ValidationException("Each item must have a product name.");
        }

        if (request.Items.Any(x => x.Quantity <= 0))
        {
            throw new ValidationException("Each item quantity must be greater than zero.");
        }

        if (request.Items.Any(x => x.UnitPrice < 0))
        {
            throw new ValidationException("Unit price cannot be negative.");
        }

        var existingOrder = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (existingOrder is null)
        {
            throw new NotFoundException($"Order with id '{id}' was not found.");
        }

        if (existingOrder.Status == OrderStatus.Shipped)
        {
            throw new ConflictException("Shipped orders cannot be updated.");
        }

        var updatedOrder = new Order
        {
            Id = existingOrder.Id,
            CreatedAtUtc = existingOrder.CreatedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow,
            Status = request.Status,
            Items = request.Items.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList()
        };

        updatedOrder.TotalAmount = updatedOrder.Items.Sum(x => x.UnitPrice * x.Quantity);

        var replaced = await _orderRepository.ReplaceAsync(updatedOrder, cancellationToken);

        return MapToResponse(replaced);
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAtUtc = order.CreatedAtUtc,
            UpdatedAtUtc = order.UpdatedAtUtc,
            Items = order.Items.Select(x => new OrderItemResponse
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList()
        };
    }
}