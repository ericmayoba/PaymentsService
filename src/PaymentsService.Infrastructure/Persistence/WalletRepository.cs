using Microsoft.EntityFrameworkCore;
using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Persistence;

public class WalletRepository : IWalletRepository
{
    private readonly PaymentsDbContext _db;

    public WalletRepository(PaymentsDbContext db)
    {
        _db = db;
    }

    public Task<Wallet?> GetByIdAsync(int id) =>
        _db.Wallets.FirstOrDefaultAsync(w => w.Id == id);

    public async Task AddAsync(Wallet wallet)
    {
        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        wallet.UpdatedAt = DateTime.UtcNow;
        _db.Wallets.Update(wallet);
        await _db.SaveChangesAsync();
    }
}
