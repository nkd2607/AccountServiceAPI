using AccountService.Models;
using System.Net;
using Xunit;
using FluentAssertions;

namespace AccountService.Tests.Integration;

public class ParallelTransferTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task ParallelTransfers_ShouldMaintainBalanceConsistency()
    {
        await fixture.ResetDatabase();

        var account1 = await CreateAccount(10000);
        var account2 = await CreateAccount(10000);
        const decimal transferAmount = 100;
        const int transferCount = 50;

        var tasks = new List<Task>();
        for (int i = 0; i < transferCount; i++)
        {
            tasks.Add(TransferAsync(account1.Id, account2.Id, transferAmount));
        }

        await Task.WhenAll(tasks);

        var finalAccount1 = await GetAccount(account1.Id);
        var finalAccount2 = await GetAccount(account2.Id);

        finalAccount1.Balance.Should().Be(10000 - transferCount * transferAmount);
        finalAccount2.Balance.Should().Be(10000 + transferCount * transferAmount);

        var transactions = await _client.GetFromJsonAsync<List<Transaction>>(
            $"/api/accounts/{account1.Id}/transactions");
        transactions.Should().HaveCount(transferCount * 2);
    }

    private async Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount)
    {
        var response = await _client.PostAsJsonAsync("/api/accounts/transfer", new
        {
            fromAccountId,
            toAccountId,
            amount
        });

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            await Task.Delay(new Random().Next(50, 150));
            response = await _client.PostAsJsonAsync("/api/accounts/transfer", new
            {
                fromAccountId,
                toAccountId,
                amount
            });
        }

        response.EnsureSuccessStatusCode();
    }

    private async Task<Account> CreateAccount(decimal balance)
    {
        var response = await _client.PostAsJsonAsync("/api/accounts", new
        {
            OwnerId = Guid.NewGuid(),
            Type = "Checking",
            Currency = "USD",
            Balance = balance,
            InterestRate = (decimal?)null,
            OpeningDate = DateTime.Today
        });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Account>() ?? throw new InvalidOperationException();
    }

    private async Task<Account> GetAccount(Guid id) =>
        await _client.GetFromJsonAsync<Account>($"/api/accounts/{id}") ?? throw new InvalidOperationException();
}