using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Handlers;

/// <summary>
/// Handles OrderCreatedEvent from Azure Service Bus.
/// Tries to reserve stock for each item. Publishes StockReservedEvent with success/failure.
/// </summary>
public class OrderCreatedEventHandler
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        IInventoryRepository inventoryRepository,
        IMessagePublisher publisher,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEventMessage orderEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing stock reservation for Order {OrderId}", orderEvent.OrderId);

        var reservations = new List<(InventoryItem Item, int Quantity)>();

        foreach (var item in orderEvent.Items)
        {
            var inventoryItem = await _inventoryRepository.GetByProductIdAsync(item.ProductId, cancellationToken);

            if (inventoryItem is null || !inventoryItem.TryReserve(item.Quantity))
            {
                // Rollback any already-reserved items
                foreach (var (reservedItem, qty) in reservations)
                {
                    reservedItem.Release(qty);
                    await _inventoryRepository.UpdateAsync(reservedItem, cancellationToken);
                }
                await _inventoryRepository.SaveChangesAsync(cancellationToken);

                await _publisher.PublishAsync(
                    new StockReservedMessage(orderEvent.OrderId, false,
                        $"Insufficient stock for product {item.ProductId}"),
                    "stock-reserved",
                    cancellationToken
                );
                return;
            }

            reservations.Add((inventoryItem, item.Quantity));
            await _inventoryRepository.UpdateAsync(inventoryItem, cancellationToken);
        }

        await _inventoryRepository.SaveChangesAsync(cancellationToken);

        await _publisher.PublishAsync(
            new StockReservedMessage(orderEvent.OrderId, true),
            "stock-reserved",
            cancellationToken
        );

        _logger.LogInformation("Stock reserved successfully for Order {OrderId}", orderEvent.OrderId);
    }
}

// Shared message contracts
public record OrderCreatedEventMessage(
    Guid OrderId,
    string CustomerId,
    string CustomerEmail,
    List<OrderCreatedItemMessage> Items,
    decimal TotalAmount,
    DateTime CreatedAt
);

public record OrderCreatedItemMessage(string ProductId, string ProductName, int Quantity, decimal UnitPrice);
public record StockReservedMessage(Guid OrderId, bool Success, string? FailureReason = null);
