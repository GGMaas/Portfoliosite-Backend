namespace Portfoliosite_Backend.Services;

public sealed record UserRegistrationResult(
    bool Succeeded,
    string? Id,
    string? Email,
    Dictionary<string, string[]> Errors);
