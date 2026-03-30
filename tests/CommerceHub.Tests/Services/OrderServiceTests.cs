using CommerceHub.Application.Common;
using CommerceHub.Application.DTOs;
using CommerceHub.Application.Interfaces;
using CommerceHub.Application.Services;
using CommerceHub.Domain.Entities;
using CommerceHub.Domain.Events;
using Moq;

namespace CommerceHub.Tests.Services;

public class OrderServiceTests
{
    private Mock<IOrderRepository> _orderRepositoryMock = null!;
private Mock<IProductRepository> _productRepositoryMock = null!;
private Mock<IEventPublisher> _eventPublisherMock = null!;
private OrderService _orderService = null!;

[SetUp]
public void SetUp()
{
    _orderRepositoryMock = new Mock<IOrderRepository>();
    _productRepositoryMock = new Mock<IProductRepository>();
    _eventPublisherMock = new Mock<IEventPublisher>();

    _orderService = new OrderService(
        _orderRepositoryMock.Object,
        _productRepositoryMock.Object,
        _eventPublisherMock.Object);
}

    [Test]
    public void CheckoutAsync_Should_ThrowValidationException_WhenQuantityIsNegative()
    {
        var request = new CheckoutOrderRequest
        {
            Items =
            [
                new CheckoutOrderItemRequest
                {
                    ProductId = "p1",
                    Quantity = -1
                }
            ]
        };

        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
            await _orderService.CheckoutAsync(request));

        Assert.That(ex!.Message, Is.EqualTo("Each item quantity must be greater than zero."));
    }

    [Test]
    public async Task CheckoutAsync_Should_DecrementStock_CreateOrder_AndPublishEvent()
    {
        var request = new CheckoutOrderRequest
        {
            Items =
            [
                new CheckoutOrderItemRequest
                {
                    ProductId = "p1",
                    Quantity = 2
                }
            ]
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new Product
                {
                    Id = "p1",
                    Name = "Phone",
                    Stock = 10,
                    Price = 100
                }
            ]);

        _productRepositoryMock
            .Setup(x => x.TryDecrementStockAsync("p1", 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _orderRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) => order);

        var result = await _orderService.CheckoutAsync(request);

        Assert.That(result.Items.Count, Is.EqualTo(1));
        Assert.That(result.TotalAmount, Is.EqualTo(200));

        _productRepositoryMock.Verify(x => x.TryDecrementStockAsync("p1", 2, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(x => x.PublishOrderCreatedAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CheckoutAsync_Should_ThrowConflictException_WhenStockIsInsufficient()
    {
        var request = new CheckoutOrderRequest
        {
            Items =
            [
                new CheckoutOrderItemRequest
                {
                    ProductId = "p1",
                    Quantity = 5
                }
            ]
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new Product
                {
                    Id = "p1",
                    Name = "Phone",
                    Stock = 2,
                    Price = 100
                }
            ]);

        var ex = Assert.ThrowsAsync<ConflictException>(async () =>
            await _orderService.CheckoutAsync(request));

        Assert.That(ex!.Message, Is.EqualTo("Insufficient stock for product 'Phone'."));

        _eventPublisherMock.Verify(x => x.PublishOrderCreatedAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void ReplaceAsync_Should_ThrowConflictException_WhenOrderIsShipped()
    {
        var request = new ReplaceOrderRequest
        {
            Status = CommerceHub.Domain.Enums.OrderStatus.Pending,
            Items =
            [
                new ReplaceOrderItemRequest
                {
                    ProductId = "p1",
                    ProductName = "Phone",
                    Quantity = 1,
                    UnitPrice = 100
                }
            ]
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync("o1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order
            {
                Id = "o1",
                Status = CommerceHub.Domain.Enums.OrderStatus.Shipped,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Items = new List<OrderItem>()
            });

        var ex = Assert.ThrowsAsync<ConflictException>(async () =>
            await _orderService.ReplaceAsync("o1", request));

        Assert.That(ex!.Message, Is.EqualTo("Shipped orders cannot be updated."));
    }

    [Test]
    public async Task ReplaceAsync_Should_ReplaceOrder_WhenOrderIsNotShipped()
    {
        var createdAt = DateTime.UtcNow.AddHours(-1);

        var request = new ReplaceOrderRequest
        {
            Status = CommerceHub.Domain.Enums.OrderStatus.Confirmed,
            Items =
            [
                new ReplaceOrderItemRequest
                {
                    ProductId = "p1",
                    ProductName = "Phone",
                    Quantity = 2,
                    UnitPrice = 150
                }
            ]
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync("o1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order
            {
                Id = "o1",
                Status = CommerceHub.Domain.Enums.OrderStatus.Pending,
                CreatedAtUtc = createdAt,
                UpdatedAtUtc = createdAt,
                Items = new List<OrderItem>()
            });

        _orderRepositoryMock
            .Setup(x => x.ReplaceAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) => order);

        var result = await _orderService.ReplaceAsync("o1", request);

        Assert.That(result.Id, Is.EqualTo("o1"));
        Assert.That(result.Status, Is.EqualTo(CommerceHub.Domain.Enums.OrderStatus.Confirmed));
        Assert.That(result.TotalAmount, Is.EqualTo(300));
        Assert.That(result.CreatedAtUtc, Is.EqualTo(createdAt));

        _orderRepositoryMock.Verify(x => x.ReplaceAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}