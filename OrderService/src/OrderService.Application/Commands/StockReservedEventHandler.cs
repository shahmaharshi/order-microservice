using OrderService.Application.Interfaces;
using OrderService.Domain.Events;

namespace OrderService.Application.Commands;

/// <summary>
/// Handles the StockReservedEvent received from InventoryService via Azure Service Bus.
/// Updates the order status based on whether stock was successfully reserved.
/// </summary>
public class StockReservedEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMessagePublisher _publisher;

    public StockReservedEventHandler(IOrderRepository orderRepository, IMessagePublisher publisher)
    {
        _orderRepository = orderRepository;
        _publisher = publisher;
    }

    public async Task HandleAsync(StockReservedEvent stockEvent, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(stockEvent.OrderId, cancellationToken);
        if (order is null) return;

        if (stockEvent.Success)
        {
            order.MarkConfirmed();
        }
        else
        {
            order.MarkFailed();
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }
}
