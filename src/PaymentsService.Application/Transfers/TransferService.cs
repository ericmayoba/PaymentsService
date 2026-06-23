using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Enums;

namespace PaymentsService.Application.Transfers;

public class TransferService
{
    private readonly IWalletRepository _wallets;
    private readonly IMovementRepository _movements;

    public TransferService(IWalletRepository wallets, IMovementRepository movements)
    {
        _wallets = wallets;
        _movements = movements;
    }

    public async Task<TransferResult> TransferAsync(int fromWalletId, int toWalletId, decimal amount)
    {
        var from = await _wallets.GetByIdAsync(fromWalletId);
        var to = await _wallets.GetByIdAsync(toWalletId);

        if (from is null || to is null)
            return TransferResult.Fail("Wallet not found");

        from.Balance = from.Balance - amount;
        to.Balance = to.Balance + amount;

        await _wallets.UpdateAsync(from);
        await _wallets.UpdateAsync(to);

        await _movements.AddAsync(new Movement(fromWalletId, amount, MovementType.Debit));
        await _movements.AddAsync(new Movement(toWalletId, amount, MovementType.Credit));

        return TransferResult.Ok();
    }
}
