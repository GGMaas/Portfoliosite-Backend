using Portfoliosite_Backend.DTOs;

namespace Portfoliosite_Backend.Services;

public interface IUserRegistrationService
{
    Task<UserRegistrationResult> RegisterAsync(
        RegisterUserDto request,
        CancellationToken cancellationToken);
}
