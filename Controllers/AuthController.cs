using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Services;

namespace Portfoliosite_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRegistrationService _userRegistrationService;

    public AuthController(IUserRegistrationService userRegistrationService)
    {
        _userRegistrationService = userRegistrationService;
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
        else
            return StatusCode(StatusCodes.Status201Created, new
            {
                result.Id,
                result.Email
            });
    }
}
