using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Entities.Identity;
using Portfoliosite_Backend.Services;

namespace Portfoliosite_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRegistrationService _userRegistrationService;
    private readonly SignInManager<User> _signInManager;

    public AuthController(
        IUserRegistrationService userRegistrationService,
        SignInManager<User> signInManager)
    {
        _userRegistrationService = userRegistrationService;
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        RegisterUserDto request,
        CancellationToken cancellationToken)
    {
        var result = await _userRegistrationService.RegisterAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(result.Errors));
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            result.Id,
            result.Email
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginUserDto request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid email or password."
            });
        }

        return NoContent();
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }
}
