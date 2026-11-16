using System.ComponentModel.DataAnnotations;

namespace Portfoliosite_Backend.DTOs;

public sealed record LoginUserDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
