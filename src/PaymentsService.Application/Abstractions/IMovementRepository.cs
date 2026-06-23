using PaymentsService.Domain.Entities;

namespace PaymentsService.Application.Abstractions;

public interface IMovementRepository
{
    Task AddAsync(Movement movement);
    Task<IReadOnlyList<Movement>> GetByWalletIdAsync(int walletId);
}
