using Moq;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.Transfers;
using PaymentsService.Domain.Entities;
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

        var service = new TransferService(walletRepo.Object, movementRepo.Object);

        // Act — 3 transfers of 0.1
        await service.TransferAsync(1, 2, 0.1m);
        await service.TransferAsync(1, 2, 0.1m);
        await service.TransferAsync(1, 2, 0.1m);

        // Assert
        Assert.Equal(99.7m, fromWallet.Balance);  // ❌ fails with double (99.69999999999999)
        Assert.Equal(50.3m, toWallet.Balance);    // ❌ fails with double (50.300000000000004)
    }
}