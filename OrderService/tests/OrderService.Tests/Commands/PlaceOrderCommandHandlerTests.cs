using Moq;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Events;
using Xunit;

namespace OrderService.Tests.Commands;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();
    private readonly Mock<IMessagePublisher> _publisherMock = new();

    private PlaceOrderCommandHandler CreateHandler()
        => new(_repoMock.Object, _publisherMock.Object);

    [Fact]
    public async Task Handle_ValidOrder_ReturnsSuccess()
    {
        var command = new PlaceOrderCommand(
            "customer-1",
            "customer@test.com",
            new List<PlaceOrderItemDto> { new("prod-1", "Widget", 2, 9.99m) }
        );

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.OrderId);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task Handle_EmptyItems_ReturnsFailure()
    {
        var command = new PlaceOrderCommand("customer-1", "customer@test.com", new List<PlaceOrderItemDto>());

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Handle_EmptyCustomerId_ReturnsFailure()
    {
        var command = new PlaceOrderCommand(
            "",
            "customer@test.com",
            new List<PlaceOrderItemDto> { new("prod-1", "Widget", 1, 9.99m) }
        );

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Handle_ValidOrder_PublishesOrderCreatedEvent()
    {
        var command = new PlaceOrderCommand(
            "customer-1",
            "customer@test.com",
            new List<PlaceOrderItemDto> { new("prod-1", "Widget", 1, 19.99m) }
        );

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.PublishAsync(It.IsAny<OrderCreatedEvent>(), "order-created", It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        _publisherMock.Verify(p => p.PublishAsync(
            It.IsAny<OrderCreatedEvent>(),
            "order-created",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidOrder_TotalAmountCalculatedCorrectly()
    {
        Order? savedOrder = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                 .Callback<Order, CancellationToken>((o, _) => savedOrder = o)
                 .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var command = new PlaceOrderCommand(
            "customer-1",
            "customer@test.com",
            new List<PlaceOrderItemDto>
            {
                new("prod-1", "Widget A", 2, 10.00m),
                new("prod-2", "Widget B", 3, 5.00m)
            }
        );

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(savedOrder);
        Assert.Equal(35.00m, savedOrder!.TotalAmount);
    }
}
