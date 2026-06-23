using NSubstitute;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.Wallets;
using PaymentsService.Domain.Entities;
using Xunit;

namespace PaymentsService.Tests.Unit;

public class WalletServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistWallet_WhenDataIsValid()
    {
        var repository = Substitute.For<IWalletRepository>();
        var service = new WalletService(repository);

        var wallet = await service.CreateAsync("0102030405", "Ana Torres", 100);

        Assert.Equal("Ana Torres", wallet.Name);
        await repository.Received(1).AddAsync(Arg.Any<Wallet>());
    }
}
