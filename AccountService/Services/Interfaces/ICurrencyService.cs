namespace AccountService.Services.Interfaces;

public interface ICurrencyService
{
    bool IsCurrencySupported(string currencyCode);
}