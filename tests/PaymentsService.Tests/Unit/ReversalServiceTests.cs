using PaymentsService.Application.Abstractions;
using PaymentsService.Application.Transfers;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Enums;
using Xunit;

public class ReversalServiceTests
{
    [Fact]
    public async Task ReverseAsync_ShouldRestoreBalances()
    {
        var fromWallet = new Wallet { Id = 1, Balance = 90.0m };
        var toWallet = new Wallet { Id = 2, Balance = 60.0m };

        var debitMovement = new Movement(1, 10.0m, MovementType.Debit) { Id = 1 };
        var creditMovement = new Movement(2, 10.0m, MovementType.Credit) { Id = 2 };

        var walletRepo = new FakeWalletRepository(fromWallet, toWallet);
        var movementRepo = new FakeMovementRepository(debitMovement, creditMovement);
        var service = new ReversalService(walletRepo, movementRepo);

        // Act
        var result = await service.ReverseAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(100.0m, fromWallet.Balance); 
        Assert.Equal(50.0m, toWallet.Balance); 
    }

    [Fact]
    public async Task ReverseAsync_AlreadyReversed_ShouldFail()
    {
        // Arrange
        var debitMovement = new Movement(1, 10.0m, MovementType.Debit)
        {
            Id = 1,
            ReversedMovementId = 1
        };

        var walletRepo = new FakeWalletRepository();
        var movementRepo = new FakeMovementRepository(debitMovement);
        var service = new ReversalService(walletRepo, movementRepo);

        // Act
        var result = await service.ReverseAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Movement already reversed", result.Error);
    }
}

// Fake wallet repository
public class FakeWalletRepository : IWalletRepository
{
    private readonly List<Wallet> _wallets;

    public FakeWalletRepository(params Wallet[] wallets)
    {
        _wallets = wallets.ToList();
    }

    public Task<Wallet?> GetByIdAsync(int id) =>
        Task.FromResult(_wallets.FirstOrDefault(w => w.Id == id));

    public Task AddAsync(Wallet wallet)
    {
        _wallets.Add(wallet);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Wallet wallet) =>
        Task.CompletedTask;
}

// Fake movement repository
public class FakeMovementRepository : IMovementRepository
{
    private readonly List<Movement> _movements;

    public FakeMovementRepository(params Movement[] movements)
    {
        _movements = movements.ToList();
    }

    public Task AddAsync(Movement movement)
    {
        _movements.Add(movement);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Movement>> GetByWalletIdAsync(int walletId) =>
        Task.FromResult<IReadOnlyList<Movement>>(
            _movements.Where(m => m.WalletId == walletId).ToList());

    public Task<Movement?> GetByIdAsync(int id) =>
        Task.FromResult(_movements.FirstOrDefault(m => m.Id == id));

    public Task UpdateAsync(Movement movement) =>
        Task.CompletedTask;
}