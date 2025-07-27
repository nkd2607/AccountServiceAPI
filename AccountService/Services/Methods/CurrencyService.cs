using AccountService.Services.Interfaces;

namespace AccountService.Services.Methods;

public class CurrencyService : ICurrencyService
{
    private readonly HashSet<string> _supportedCurrencies = new(StringComparer.OrdinalIgnoreCase) { "RUB", "USD", "EUR" }; 
    public bool IsCurrencySupported(string currencyCode) => _supportedCurrencies.Contains(currencyCode);
}