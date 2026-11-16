using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfoliosite_Backend.Data.Database_entities;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Entities.Identity;

namespace Portfoliosite_Backend.Services;

public sealed class UserRegistrationService : IUserRegistrationService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _dbContext;

    public UserRegistrationService(UserManager<User> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<UserRegistrationResult> RegisterAsync(
        RegisterUserDto request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            name = request.Name
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CreateFailureResult(createResult);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Gast);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CreateFailureResult(roleResult);
        }

        await transaction.CommitAsync(cancellationToken);

        return new UserRegistrationResult(
            true,
            user.Id,
            user.Email,
            new Dictionary<string, string[]>());
    }

    private static UserRegistrationResult CreateFailureResult(IdentityResult identityResult)
    {
        var errors = identityResult.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        return new UserRegistrationResult(
            false,
            null,
            null,
            errors);
    }
}
