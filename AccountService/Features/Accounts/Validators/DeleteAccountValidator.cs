using AccountService.Features.Accounts.Commands;
using AccountService.Services.Interfaces;
using FluentValidation;

namespace AccountService.Features.Accounts.Validators;

public class DeleteAccountValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountValidator(IAccountStorageService storage)
    {
        RuleFor(x => x.Id).NotEmpty().Must(id => storage.GetAccount(id) != null).WithMessage("—чЄт не найден");
    }
}