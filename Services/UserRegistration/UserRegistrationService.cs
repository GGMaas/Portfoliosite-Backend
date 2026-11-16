using Microsoft.AspNetCore.Identity;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Entities.Identity;

namespace Portfoliosite_Backend.Services;

public sealed class UserRegistrationService : IUserRegistrationService
{
    private readonly UserManager<User> _userManager;

    public UserRegistrationService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserRegistrationResult> RegisterAsync(
        RegisterUserDto request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            name = request.Name
        };

        var identityResult = await _userManager.CreateAsync(user, request.Password);
        var errors = identityResult.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        return new UserRegistrationResult(
            identityResult.Succeeded,
            user.Id,
            user.Email,
            errors);
    }
}
