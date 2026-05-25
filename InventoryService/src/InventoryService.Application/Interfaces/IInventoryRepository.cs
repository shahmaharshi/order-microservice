using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string topicName, CancellationToken cancellationToken = default) where T : class;
}
