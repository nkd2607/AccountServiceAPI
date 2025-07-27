using AccountService.Services.Interfaces;

namespace AccountService.Services.Methods;

public class ClientVerificationService : IClientVerificationService
{
    public bool VerifyClientExists(Guid clientId) => true;
}