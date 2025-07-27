using AccountService.Features.Transactions.Commands;
using AccountService.Services.Interfaces;
using FluentValidation;

namespace AccountService.Features.Transactions.Validators;

public class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator(ICurrencyService currencyService, IAccountStorageService accountStorage)
    {
        RuleFor(cmd => cmd.FromAccountId)
            .NotEmpty()
            .Must(id => accountStorage.GetAccount(id) != null)
            .WithMessage("����-����������� �� ������");

        RuleFor(cmd => cmd.ToAccountId)
            .NotEmpty()
            .Must(id => accountStorage.GetAccount(id) != null)
            .WithMessage("����-���������� �� ������")
            .NotEqual(cmd => cmd.FromAccountId)
            .WithMessage("������ ������� ������� �� ��� �� ����");

        RuleFor(cmd => cmd.Amount).GreaterThan(0);

        RuleFor(cmd => cmd.Currency)
            .Must(currencyService.IsCurrencySupported)
            .WithMessage("������ �� ��������������");

        RuleFor(cmd => cmd.Description).NotEmpty().MaximumLength(255);
    }
}