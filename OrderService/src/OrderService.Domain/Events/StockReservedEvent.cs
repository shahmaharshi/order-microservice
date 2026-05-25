namespace OrderService.Domain.Events;

public record StockReservedEvent(Guid OrderId, bool Success, string? FailureReason = null);
