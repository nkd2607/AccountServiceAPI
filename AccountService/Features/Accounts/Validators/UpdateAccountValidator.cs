using AccountService.Features.Accounts.Commands;
using AccountService.Models.Enums;
using AccountService.Services.Interfaces;
using FluentValidation;

namespace AccountService.Features.Accounts.Validators;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator(IClientVerificationService clientVerification, ICurrencyService currencyService)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .Must(clientVerification.VerifyClientExists)
            .WithMessage("������ �� ������");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(currencyService.IsCurrencySupported)
            .WithMessage("������ �� ��������������");

        RuleFor(x => x.InterestRate)
            .NotEmpty()
            .When(x => x.Type != AccountType.Checking)
            .WithMessage("��� �������� ������/������� ��������� ���������� ������");

        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
    }
}