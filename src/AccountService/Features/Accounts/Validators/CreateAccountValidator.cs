using AccountService.Features.Accounts.Commands;
using AccountService.Models.Enums;
using AccountService.Services.Interfaces;
using FluentValidation;
namespace AccountService.Features.Accounts.Validators; public class CreateAccountValidator : AbstractValidator<CreateAccountCommand> { public CreateAccountValidator(IClientVerificationService clientVerification, ICurrencyService currencyService) { RuleFor(x => x.OwnerId).NotEmpty().Must(clientVerification.VerifyClientExists).WithMessage("Клиент не найден"); RuleFor(x => x.Currency).NotEmpty().Must(currencyService.IsCurrencySupported).WithMessage("Валюта не поддерживается"); RuleFor(x => x.InterestRate).NotEmpty().When(x => x.Type != AccountType.Checking).WithMessage("Необходима процентная ставка"); } }