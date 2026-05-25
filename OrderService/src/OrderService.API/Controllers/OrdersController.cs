using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Queries;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Places a new order. Publishes OrderCreated event to Azure Service Bus.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlaceOrderResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken ct)
    {
        var command = new PlaceOrderCommand(request.CustomerId, request.CustomerEmail, request.Items);
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
    }

    /// <summary>Gets a single order by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id), ct);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Gets all orders for a customer.</summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(string customerId, CancellationToken ct)
    {
        var orders = await _mediator.Send(new GetOrdersByCustomerQuery(customerId), ct);
        return Ok(orders);
    }

    /// <summary>Cancels a pending or stock-confirmed order.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelOrderCommand(id, User.Identity?.Name ?? "unknown"), ct);
        if (!result.Success) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
