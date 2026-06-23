using Moq;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.Transfers;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Enums;
using Xunit;

namespace PaymentsService.Tests.Unit;

public class TransferServiceTests
{
    [Fact]
    public async Task TransferAsync_BalanceShouldBeExactAfterDecimalTransfers()
    {
        // Arrange
        var fromWallet = new Wallet { Id = 1, Balance = 100.0m };
        var toWallet = new Wallet { Id = 2, Balance = 50.0m};

        var walletRepo = new Mock<IWalletRepository>();
        var movementRepo = new Mock<IMovementRepository>();

        walletRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fromWallet);
        walletRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(toWallet);

        var idempotencyRepo = new Mock<IIdempotencyRepository>();
        idempotencyRepo
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        var uow = new Mock<IUnitOfWork>();
        var service = new TransferService(walletRepo.Object, movementRepo.Object, idempotencyRepo.Object, uow);

        // Act — 3 transfers of 0.1
        var key = Guid.NewGuid();
        await service.TransferAsync(1, 2, 0.1m, key);
        await service.TransferAsync(1, 2, 0.1m, Guid.NewGuid()); // key diferente cada vez
        await service.TransferAsync(1, 2, 0.1m,Guid.NewGuid());

        // Assert
        Assert.Equal(99.7m, fromWallet.Balance);  // ❌ fails with double (99.69999999999999)
        Assert.Equal(50.3m, toWallet.Balance);    // ❌ fails with double (50.300000000000004)
    }

    [Fact]
    public async Task TransferAsync_SameIdempotencyKey_ShouldOnlyProcessOnce()
    {
        // Arrange
        var fromWallet = new Wallet { Id = 1, Balance = 100.0m };
        var toWallet = new Wallet { Id = 2, Balance = 50.0m };

        var walletRepo = new Mock<IWalletRepository>();
        var movementRepo = new Mock<IMovementRepository>();
        var idempotencyRepo = new Mock<IIdempotencyRepository>();

        walletRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fromWallet);
        walletRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(toWallet);

        // First call: key does not exist → process transfer
        // Subsequent calls: key already exists → block execution
        var callCount = 0;
        idempotencyRepo
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => callCount++ > 0); // false on first call, true on subsequent ones

        var uow = new Mock<IUnitOfWork>();
        var service = new TransferService(walletRepo.Object, movementRepo.Object, idempotencyRepo.Object, uow.Object);
        var sameKey = Guid.NewGuid(); // same key used for both calls

        // Act
        await service.TransferAsync(1, 2, 10.0m, sameKey); // first call  → processed
        await service.TransferAsync(1, 2, 10.0m, sameKey); // second call → blocked

        // Assert — balances should only reflect ONE transfer
        Assert.Equal(90.0m, fromWallet.Balance); // 100 - 10
        Assert.Equal(60.0m, toWallet.Balance);   // 50  + 10

        // Movement should only be registered once (1 debit + 1 credit, not 4)
        movementRepo.Verify(
            r => r.AddAsync(It.IsAny<Movement>()),
            Times.Exactly(2)
        );

        // Idempotency key should only be saved once
        idempotencyRepo.Verify(
            r => r.SaveAsync(It.IsAny<Guid>()),
            Times.Once
        );
    }

    [Fact]
    public async Task TransferAsync_WhenCreditFails_ShouldRollbackDebit()
    {
        // Arrange
        var fromWallet = new Wallet { Id = 1, Balance = 100.0m };
        var toWallet = new Wallet { Id = 2, Balance = 50.0m };

        var walletRepo = new Mock<IWalletRepository>();
        var movementRepo = new Mock<IMovementRepository>();
        var idempotencyRepo = new Mock<IIdempotencyRepository>();
        var uow = new Mock<IUnitOfWork>();

        walletRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fromWallet);
        walletRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(toWallet);

        // Simulate failure when adding the credit movement
        movementRepo
            .Setup(r => r.AddAsync(It.Is<Movement>(m => m.Type == MovementType.Credit)))
            .ThrowsAsync(new Exception("DB error"));

        var service = new TransferService(
            walletRepo.Object,
            movementRepo.Object,
            idempotencyRepo.Object,
            uow.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() =>
            service.TransferAsync(1, 2, 10.0m, Guid.NewGuid()));

        // Assert — both balances must remain unchanged
        Assert.Equal(100.0m, fromWallet.Balance);
        Assert.Equal(50.0m, toWallet.Balance);

        // Transaction must have been rolled back
        uow.Verify(u => u.RollbackAsync(), Times.Once);

        // Commit must never have been called
        uow.Verify(u => u.CommitAsync(), Times.Never);

        // Idempotency key must not have been saved
        idempotencyRepo.Verify(r => r.SaveAsync(It.IsAny<Guid>()), Times.Never);
    }
}