namespace OrderService.Application.DTOs;

public record OrderDto(
    Guid Id,
    string CustomerId,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OrderItemDto> Items
);

public record OrderItemDto(string ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record PlaceOrderItemDto(string ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record PlaceOrderRequest(
    string CustomerId,
    string CustomerEmail,
    List<PlaceOrderItemDto> Items
);
