using PaymentsService.Domain.Entities;

namespace PaymentsService.Application.Abstractions;

public interface IMovementRepository
{
    Task AddAsync(Movement movement);
    Task<IReadOnlyList<Movement>> GetByWalletIdAsync(int walletId);
    Task<Movement?> GetByIdAsync(int id);
    Task UpdateAsync(Movement movement);
}
