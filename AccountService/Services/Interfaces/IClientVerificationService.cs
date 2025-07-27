namespace AccountService.Services.Interfaces;

public interface IClientVerificationService
{
    bool VerifyClientExists(Guid clientId);
}