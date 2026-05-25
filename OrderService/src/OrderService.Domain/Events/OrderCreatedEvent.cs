namespace OrderService.Domain.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    string CustomerId,
    string CustomerEmail,
    List<OrderCreatedItem> Items,
    decimal TotalAmount,
    DateTime CreatedAt
);

public record OrderCreatedItem(string ProductId, string ProductName, int Quantity, decimal UnitPrice);
