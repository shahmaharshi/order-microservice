using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Handlers;

public record OrderConfirmedMessage(
    Guid OrderId,
    string CustomerId,
    string CustomerEmail,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<OrderConfirmedItem> Items
);
public record OrderConfirmedItem(string ProductName, int Quantity, decimal UnitPrice);

public class OrderConfirmedNotificationHandler
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OrderConfirmedNotificationHandler> _logger;

    public OrderConfirmedNotificationHandler(IEmailSender emailSender, ILogger<OrderConfirmedNotificationHandler> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task HandleAsync(OrderConfirmedMessage message, CancellationToken cancellationToken = default)
    {
        var itemLines = string.Join("\n", message.Items.Select(i => $"  - {i.ProductName} x{i.Quantity} @ ${i.UnitPrice:F2}"));

        var body = $"""
            Dear Customer,

            Your order has been confirmed!

            Order ID: {message.OrderId}
            Total: ${message.TotalAmount:F2}
            Date: {message.CreatedAt:yyyy-MM-dd HH:mm} UTC

            Items:
            {itemLines}

            Thank you for your order.
            Order Processing System
            """;

        await _emailSender.SendAsync(
            to: message.CustomerEmail,
            subject: $"Order Confirmed - #{message.OrderId.ToString()[..8].ToUpper()}",
            body: body,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Confirmation email sent for Order {OrderId} to {Email}", message.OrderId, message.CustomerEmail);
    }
}
