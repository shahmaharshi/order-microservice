using Moq;
using OrderService.Application.Interfaces;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Tests.Queries;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();

    [Fact]
    public async Task Handle_ExistingOrder_ReturnsOrderDto()
    {
        var order = Order.Create("cust-1", "test@test.com",
            new List<OrderItem> { OrderItem.Create("p1", "Product 1", 1, 10m) });

        _repoMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(order);

        var handler = new GetOrderByIdQueryHandler(_repoMock.Object);
        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(order.Id, result!.Id);
        Assert.Equal("cust-1", result.CustomerId);
        Assert.Equal("Pending", result.Status);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Handle_NonExistentOrder_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Order?)null);

        var handler = new GetOrderByIdQueryHandler(_repoMock.Object);
        var result = await handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
