using MediatR;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Events;

namespace OrderService.Application.Commands;

// Command
public record PlaceOrderCommand(
    string CustomerId,
    string CustomerEmail,
    List<PlaceOrderItemDto> Items
) : IRequest<PlaceOrderResult>;

// Result
public record PlaceOrderResult(bool Success, Guid? OrderId, string? Error);

// Handler
public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMessagePublisher _publisher;

    public PlaceOrderCommandHandler(IOrderRepository orderRepository, IMessagePublisher publisher)
    {
        _orderRepository = orderRepository;
        _publisher = publisher;
    }

    public async Task<PlaceOrderResult> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var items = request.Items
                .Select(i => OrderItem.Create(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
                .ToList();

            var order = Order.Create(request.CustomerId, request.CustomerEmail, items);

            await _orderRepository.AddAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            // Publish OrderCreated event to Azure Service Bus
            var orderCreatedEvent = new OrderCreatedEvent(
                order.Id,
                order.CustomerId,
                order.CustomerEmail,
                order.Items.Select(i => new OrderCreatedItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList(),
                order.TotalAmount,
                order.CreatedAt
            );

            await _publisher.PublishAsync(orderCreatedEvent, "order-created", cancellationToken);

            return new PlaceOrderResult(true, order.Id, null);
        }
        catch (ArgumentException ex)
        {
            return new PlaceOrderResult(false, null, ex.Message);
        }
    }
}
