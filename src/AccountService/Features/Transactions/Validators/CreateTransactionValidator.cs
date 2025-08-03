using AccountService.Features.Transactions.Commands;
using AccountService.Services.Interfaces;
using FluentValidation;
namespace AccountService.Features.Transactions.Validators; public class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand> { public CreateTransactionValidator(ICurrencyService currencyService) { RuleFor(cmd => cmd.AccountId).NotEmpty(); RuleFor(cmd => cmd.Sum).GreaterThan(0); RuleFor(cmd => cmd.Currency).Must(currencyService.IsCurrencySupported).WithMessage("Валюта не поддерживается"); RuleFor(cmd => cmd.Description).NotEmpty().MaximumLength(255); } }