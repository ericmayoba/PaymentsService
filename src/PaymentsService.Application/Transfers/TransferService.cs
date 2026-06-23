using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Enums;

namespace PaymentsService.Application.Transfers;

public class TransferService
{
    private readonly IWalletRepository _wallets;
    private readonly IMovementRepository _movements;
        private readonly IIdempotencyRepository _idempotency;
    private readonly IUnitOfWork _uow;

    public TransferService(IWalletRepository wallets, IMovementRepository movements,IIdempotencyRepository idempotency,
    IUnitOfWork uow)
    {
        _wallets = wallets;
        _movements = movements;
        _idempotency = idempotency;
        _uow = uow;
    }

    public async Task<TransferResult> TransferAsync(int fromWalletId, int toWalletId, decimal amount, Guid idempotencyKey)
    {
        if (await _idempotency.ExistsAsync(idempotencyKey))
            return TransferResult.Ok();

        var from = await _wallets.GetByIdAsync(fromWalletId);
        var to = await _wallets.GetByIdAsync(toWalletId);

        if (from is null || to is null)
            return TransferResult.Fail("Wallet not found");

        await _uow.BeginTransactionAsync();
        try
        {
            from.Balance -= amount;
            to.Balance += amount;

            await _wallets.UpdateAsync(from);
            await _wallets.UpdateAsync(to);

            await _movements.AddAsync(new Movement(fromWalletId, amount, MovementType.Debit));
            await _movements.AddAsync(new Movement(toWalletId, amount, MovementType.Credit));

            await _idempotency.SaveAsync(idempotencyKey);
            await _uow.CommitAsync();

            return TransferResult.Ok();
        }
        catch
        {
            await _uow.RollbackAsync(); 
            throw;
        }
    }
}
