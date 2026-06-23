using Microsoft.EntityFrameworkCore;
using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;


namespace PaymentsService.Infrastructure.Persistence;
public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly PaymentsDbContext _db;

    public IdempotencyRepository(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsAsync(Guid key)
    {
        return await _db.IdempotencyKeys
            .AnyAsync((IdempotencyKey k) => k.Key == key);
    }

    public async Task SaveAsync(Guid key)
    {
        _db.IdempotencyKeys.Add(new IdempotencyKey(key));
        await _db.SaveChangesAsync();
    }
}
