using AccountService.Features.Accounts.Commands;
using AccountService.Models;
using AccountService.Models.Enums;
using AccountService.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AccountService.Tests.Unit;

public class AccountBusinessLogicTests
{
    private readonly Mock<IAccountStorageService> _storageMock = new();
    private readonly Mock<IClientVerificationService> _clientMock = new();
    private readonly Mock<ICurrencyService> _currencyMock = new();
    private readonly UpdateAccountHandler _handler;

    public AccountBusinessLogicTests()
    {
        _handler = new UpdateAccountHandler(
            _storageMock.Object,
            _clientMock.Object,
            _currencyMock.Object);
    }

    [Fact]
    public async Task UpdateAccount_InvalidClosingDate_ReturnsFailure()
    {
        var account = CreateAccount(openingDate: DateTime.Today);
        var command = new UpdateAccountCommand(
            account.Id, account.OwnerId, account.Type, account.Currency,
            account.Balance, account.InterestRate, DateTime.Today.AddDays(-1),
            account.Version);

        _storageMock.Setup(x => x.GetAccount(account.Id)).Returns(account);
        _clientMock.Setup(x => x.VerifyClientExists(account.OwnerId)).Returns(true);
        _currencyMock.Setup(x => x.IsCurrencySupported(account.Currency)).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Error.Should().Contain("Дата закрытия");
    }

    [Fact]
    public async Task UpdateAccount_ConcurrencyConflict_Returns409()
    {
        var account = CreateAccount(version: 100);
        var command = new UpdateAccountCommand(
            account.Id, account.OwnerId, account.Type, account.Currency,
            account.Balance, account.InterestRate, null,
            Version: 99);

        _storageMock.Setup(x => x.GetAccount(account.Id)).Returns(account);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Error.Should().Contain("изменён другим пользователем");
    }

    [Fact]
    public async Task UpdateAccount_ValidCreditAccount_RequiresInterestRate()
    {
        var account = CreateAccount(type: AccountType.Credit, interestRate: 0.025m);
        var command = new UpdateAccountCommand(
            account.Id, account.OwnerId, AccountType.Credit, account.Currency,
            account.Balance, null, null, account.Version); // Missing interest rate

        _storageMock.Setup(x => x.GetAccount(account.Id)).Returns(account);
        _clientMock.Setup(x => x.VerifyClientExists(account.OwnerId)).Returns(true);
        _currencyMock.Setup(x => x.IsCurrencySupported(account.Currency)).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Error.Should().Contain("процентная ставка");
    }

    private static Account CreateAccount(
        DateTime? openingDate = null,
        AccountType type = AccountType.Checking,
        decimal? interestRate = null,
        uint version = 1)
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Type = type,
            Currency = "USD",
            Balance = 1000,
            InterestRate = interestRate,
            OpeningDate = openingDate ?? DateTime.Today.AddDays(-30),
            ClosingDate = null,
            Version = version
        };
    }
}