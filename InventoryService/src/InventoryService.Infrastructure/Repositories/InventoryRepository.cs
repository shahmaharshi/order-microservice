using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context) => _context = context;

    public async Task<InventoryItem?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
        => await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);

    public async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.InventoryItems.ToListAsync(cancellationToken);

    public async Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
        => await _context.InventoryItems.AddAsync(item, cancellationToken);

    public Task UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        _context.InventoryItems.Update(item);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
