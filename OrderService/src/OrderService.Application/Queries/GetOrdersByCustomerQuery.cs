using MediatR;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Queries;

public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<List<OrderDto>>;

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        return orders.Select(order => new OrderDto(
            order.Id,
            order.CustomerId,
            order.CustomerEmail,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList()
        )).ToList();
    }
}
