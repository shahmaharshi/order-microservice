namespace OrderService.Domain.Entities;

public enum OrderStatus
{
    Pending = 0,
    StockConfirmed = 1,
    Confirmed = 2,
    Cancelled = 3,
    Failed = 4
}
