using AccountService.Data;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Services.Methods;

public class InterestAccrualService(
    AccountServiceContext context,
    ILogger<InterestAccrualService> logger)
{
    public async Task AccrueInterestForAllAccounts()
    {
        try
        {
            logger.LogInformation("Начало ежедневного начисления процентов");

            const int batchSize = 100;
            var lastProcessedDate = DateTime.MinValue;
            var lastProcessedId = Guid.Empty;
            var accountsProcessed = 0;

            while (true)
            {
                var date = lastProcessedDate;
                var id = lastProcessedId;
                var accounts = await context.Accounts
                    .Where(a => a.OpeningDate > date ||
                                // ReSharper disable once EntityFramework.UnsupportedServerSideFunctionCall
                                (a.OpeningDate == date && a.Id.CompareTo(id) > 0))
                    .Where(a => a.ClosingDate == null &&
                                a.InterestRate > 0 &&
                                a.Balance > 0)
                    .OrderBy(a => a.OpeningDate)
                    .ThenBy(a => a.Id)
                    .Take(batchSize)
                    .AsNoTracking()
                    .ToListAsync();

                if (accounts.Count == 0) break;

                foreach (var account in accounts)
                {
                    try
                    {
                        await context.Database.ExecuteSqlInterpolatedAsync(
                            $"SELECT accrue_interest({account.Id})");
                        accountsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex,
                            $"Ошибка в начислении процентов на счёт {account.Id}");
                    }

                    lastProcessedDate = account.OpeningDate;
                    lastProcessedId = account.Id;
                }
            }

            logger.LogInformation(
                $"Завершено начисление процентов на {accountsProcessed} счетов");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Ошибка во время начисления процентов");
            throw;
        }
    }
}