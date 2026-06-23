using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Enums;

namespace PaymentsService.Application.Transfers;

public class ReversalService
{
    private readonly IWalletRepository _wallets;
    private readonly IMovementRepository _movements;

    public ReversalService(IWalletRepository wallets, IMovementRepository movements)
    {
        _wallets = wallets;
        _movements = movements;
    }

    public async Task<TransferResult> ReverseAsync(int debitMovementId)
    {
        // 1. Find the original movement
        var debitMovement = await _movements.GetByIdAsync(debitMovementId);

        if (debitMovement is null)
            return TransferResult.Fail("Movement not found");

        if (debitMovement.Type != MovementType.Debit)
            return TransferResult.Fail("Only debit movements can be reversed");

        // 2. Idempotency — prevent double reversal
        if (debitMovement.ReversedMovementId is not null)
            return TransferResult.Fail("Movement already reversed");

        // 3. Find origin wallet
        var fromWallet = await _wallets.GetByIdAsync(debitMovement.WalletId);
        if (fromWallet is null)
            return TransferResult.Fail("Origin wallet not found");

        // 4. Find associated credit movement
        var allMovements = await _movements.GetByWalletIdAsync(debitMovement.WalletId);
        var creditMovement = allMovements
            .FirstOrDefault(m =>
                m.Type == MovementType.Credit &&
                m.Amount == debitMovement.Amount &&
                m.ReversedMovementId is null);

        if (creditMovement is null)
            return TransferResult.Fail("Associated credit movement not found");

        // 5. Find destination wallet
        var toWallet = await _wallets.GetByIdAsync(creditMovement.WalletId);
        if (toWallet is null)
            return TransferResult.Fail("Destination wallet not found");

        // 6. Validate balance — destination cannot go negative
        if (toWallet.Balance - debitMovement.Amount < 0)
            return TransferResult.Fail("Insufficient balance to reverse");

        // 7. Restore balances
        fromWallet.Balance += debitMovement.Amount;
        toWallet.Balance -= debitMovement.Amount;

        await _wallets.UpdateAsync(fromWallet);
        await _wallets.UpdateAsync(toWallet);

        // 8. Register reversal movements
        await _movements.AddAsync(new Movement(fromWallet.Id, debitMovement.Amount, MovementType.Credit, debitMovementId));
        await _movements.AddAsync(new Movement(toWallet.Id, debitMovement.Amount, MovementType.Debit, debitMovementId));

        // 9. Mark original movement as reversed
        debitMovement.ReversedMovementId = debitMovementId;
        await _movements.UpdateAsync(debitMovement);

        return TransferResult.Ok();
    }
}