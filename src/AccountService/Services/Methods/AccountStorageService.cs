using AccountService.Models;
using AccountService.Services.Interfaces;

namespace AccountService.Services.Methods;

public class AccountStorageService : IAccountStorageService
{
    private readonly List<Account> _accounts = [];

    public Account CreateAccount(Account account)
    {
        account.Id = Guid.NewGuid();
        account.OpeningDate = DateTime.UtcNow;
        _accounts.Add(account);
        return account;
    }

    public bool DeleteAccount(Guid id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        if (account == null) return false;
        return _accounts.Remove(account);
    }

    public Account? GetAccount(Guid id)
    {
        return _accounts.FirstOrDefault(a => a.Id == id);
    }

    public IEnumerable<Account> GetAccounts()
    {
        return _accounts;
    }

    public Account? UpdateAccount(Account account)
    {
        var existing = _accounts.FirstOrDefault(a => a.Id == account.Id);
        if (existing == null) return null;
        existing.OwnerId = account.OwnerId;
        existing.Type = account.Type;
        existing.Currency = account.Currency;
        existing.Balance = account.Balance;
        existing.InterestRate = account.InterestRate;
        existing.ClosingDate = account.ClosingDate;
        return existing;
    }
}