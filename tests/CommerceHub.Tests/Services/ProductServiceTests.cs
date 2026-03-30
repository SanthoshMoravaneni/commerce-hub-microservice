using CommerceHub.Application.Common;
using CommerceHub.Application.DTOs;
using CommerceHub.Application.Interfaces;
using CommerceHub.Application.Services;
using CommerceHub.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CommerceHub.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _productService = new ProductService(_productRepositoryMock.Object);
    }

    [Test]
    public async Task AdjustStockAsync_Should_ThrowValidationException_WhenDeltaIsZero()
    {
        var request = new AdjustStockRequest { Delta = 0 };

        var act = async () => await _productService.AdjustStockAsync("p1", request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Stock delta cannot be zero.");
    }

    [Test]
    public async Task AdjustStockAsync_Should_ThrowNotFoundException_WhenProductDoesNotExist()
    {
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var request = new AdjustStockRequest { Delta = -2 };

        var act = async () => await _productService.AdjustStockAsync("p1", request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product with id 'p1' was not found.");
    }

    [Test]
    public async Task AdjustStockAsync_Should_ThrowConflictException_WhenStockWouldBecomeNegative()
    {
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = "p1",
                Name = "Phone",
                Stock = 1,
                Price = 500
            });

        _productRepositoryMock
            .Setup(x => x.AdjustStockAsync("p1", -2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var request = new AdjustStockRequest { Delta = -2 };

        var act = async () => await _productService.AdjustStockAsync("p1", request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Stock adjustment failed because it would result in negative stock.");
    }

    [Test]
    public async Task AdjustStockAsync_Should_ReturnUpdatedProduct_WhenAdjustmentSucceeds()
    {
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = "p1",
                Name = "Phone",
                Stock = 5,
                Price = 500
            });

        _productRepositoryMock
            .Setup(x => x.AdjustStockAsync("p1", -2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = "p1",
                Name = "Phone",
                Stock = 3,
                Price = 500,
                UpdatedAtUtc = DateTime.UtcNow
            });

        var request = new AdjustStockRequest { Delta = -2 };

        var result = await _productService.AdjustStockAsync("p1", request);

        result.Id.Should().Be("p1");
        result.Stock.Should().Be(3);
        result.Name.Should().Be("Phone");
    }
}