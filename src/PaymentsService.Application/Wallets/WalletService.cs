using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Application.Wallets;

public class WalletService
{
    private readonly IWalletRepository _wallets;

    public WalletService(IWalletRepository wallets)
    {
        _wallets = wallets;
    }

    public async Task<Wallet> CreateAsync(string documentId, string name, double initialBalance)
    {
        var wallet = new Wallet
        {
            DocumentId = documentId,
            Name = name,
            Balance = initialBalance,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _wallets.AddAsync(wallet);
        return wallet;
    }

    public Task<Wallet?> GetAsync(int id) => _wallets.GetByIdAsync(id);
}
