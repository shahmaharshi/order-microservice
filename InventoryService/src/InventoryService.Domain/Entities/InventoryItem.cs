namespace InventoryService.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; private set; }
    public string ProductId { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private InventoryItem() { }

    public static InventoryItem Create(string productId, string productName, int initialQuantity)
    {
        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            AvailableQuantity = initialQuantity,
            ReservedQuantity = 0,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public bool TryReserve(int quantity)
    {
        if (AvailableQuantity < quantity) return false;
        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void Release(int quantity)
    {
        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
