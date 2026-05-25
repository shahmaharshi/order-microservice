using MediatR;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Commands;

public record CancelOrderCommand(Guid OrderId, string RequestedBy) : IRequest<CancelOrderResult>;
public record CancelOrderResult(bool Success, string? Error);

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return new CancelOrderResult(false, "Order not found.");

        if (order.Status == Domain.Entities.OrderStatus.Confirmed)
            return new CancelOrderResult(false, "Confirmed orders cannot be cancelled.");

        order.Cancel();
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return new CancelOrderResult(true, null);
    }
}
