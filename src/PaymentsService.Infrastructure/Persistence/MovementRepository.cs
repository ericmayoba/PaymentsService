using Microsoft.EntityFrameworkCore;
using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Persistence;

public class MovementRepository : IMovementRepository
{
    private readonly PaymentsDbContext _db;

    public MovementRepository(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Movement movement)
    {
        _db.Movements.Add(movement);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Movement>> GetByWalletIdAsync(int walletId) =>
        await _db.Movements
            .Where(m => m.WalletId == walletId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
}
