namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public List<OrderItem> Items { get; private set; } = new();
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private Order() { }

    public static Order Create(string customerId, string customerEmail, List<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("CustomerId is required.");
        if (string.IsNullOrWhiteSpace(customerEmail)) throw new ArgumentException("CustomerEmail is required.");
        if (items == null || items.Count == 0) throw new ArgumentException("Order must have at least one item.");

        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            Items = items,
            Status = OrderStatus.Pending,
            TotalAmount = items.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void ConfirmStock() { Status = OrderStatus.StockConfirmed; UpdatedAt = DateTime.UtcNow; }
    public void MarkConfirmed() { Status = OrderStatus.Confirmed; UpdatedAt = DateTime.UtcNow; }
    public void Cancel() { Status = OrderStatus.Cancelled; UpdatedAt = DateTime.UtcNow; }
    public void MarkFailed() { Status = OrderStatus.Failed; UpdatedAt = DateTime.UtcNow; }
}
