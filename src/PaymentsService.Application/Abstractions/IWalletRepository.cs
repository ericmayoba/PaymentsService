using PaymentsService.Domain.Entities;

namespace PaymentsService.Application.Abstractions;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(int id);
    Task AddAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
}
