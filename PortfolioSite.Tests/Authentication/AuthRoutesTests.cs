using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Portfoliosite_Backend.Data.Database_entities;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Entities.Identity;
using PortfolioSite.Tests.Infrastructure;

namespace PortfolioSite.Tests.Authentication;

public sealed class AuthRoutesTests : IAsyncLifetime
{
    private PortfolioApiFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new PortfolioApiFactory(useTestAuthentication: false);
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_creates_account_with_guest_role_and_does_not_log_in()
    {
        // Arrange
        var request = CreateRegistration("new.user@example.test", "ValidPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);
        var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
        var user = await FindUserAsync(request.Email);
        var roles = user is null
            ? Array.Empty<string>()
            : await GetRolesAsync(user);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(request.Email, responseBody.GetProperty("email").GetString());
        Assert.Equal(user?.Id, responseBody.GetProperty("id").GetString());
        Assert.NotNull(user);
        Assert.Contains(UserRoles.Gast, roles);
        Assert.DoesNotContain(roles, role => role == UserRoles.Admin || role == UserRoles.Editor);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Register_returns_bad_request_for_password_rejected_by_identity()
    {
        // Arrange
        var request = CreateRegistration("weak.password@example.test", "weak");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);
        var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
        var user = await FindUserAsync(request.Email);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(responseBody.GetProperty("errors").EnumerateObject().Any());
        Assert.Null(user);
    }

    [Theory]
    [InlineData("not-an-email", "ValidPassword123!")]
    [InlineData("", "ValidPassword123!")]
    public async Task Register_returns_bad_request_for_invalid_email(string email, string password)
    {
        // Arrange
        var request = CreateRegistration(email, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_returns_bad_request_when_email_is_already_registered()
    {
        // Arrange
        var request = CreateRegistration("duplicate.user@example.test", "ValidPassword123!");
        var firstResponse = await _client.PostAsJsonAsync("/api/Auth/register", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        var duplicateResponse = await _client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Login_sets_authentication_cookie_for_registered_user()
    {
        // Arrange
        var registration = CreateRegistration("login.user@example.test", "ValidPassword123!");
        var registrationResponse = await _client.PostAsJsonAsync("/api/Auth/register", registration);
        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);
        var login = new LoginUserDto
        {
            Email = registration.Email,
            Password = registration.Password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", login);
        var hasSetCookie = response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders);
        var protectedResponse = await _client.PostAsJsonAsync(
            "/api/Projects",
            new CreateProjectDto { Name = "Should be blocked for Gast" });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(hasSetCookie);
        Assert.Contains(setCookieHeaders!, cookie =>
            cookie.Contains("Identity.Application", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.Forbidden, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task Login_returns_unauthorized_for_incorrect_password()
    {
        // Arrange
        var registration = CreateRegistration("wrong.password@example.test", "ValidPassword123!");
        var registrationResponse = await _client.PostAsJsonAsync("/api/Auth/register", registration);
        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);
        var login = new LoginUserDto
        {
            Email = registration.Email,
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_returns_unauthorized_for_unknown_email()
    {
        // Arrange
        var login = new LoginUserDto
        {
            Email = "missing.user@example.test",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("invalid-email", "ValidPassword123!")]
    [InlineData("valid.user@example.test", "")]
    public async Task Login_returns_bad_request_for_invalid_model(string email, string password)
    {
        // Arrange
        var login = new LoginUserDto
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static RegisterUserDto CreateRegistration(string email, string password)
    {
        return new RegisterUserDto
        {
            Email = email,
            Password = password,
            Name = "Test user"
        };
    }

    private async Task<User?> FindUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        return await userManager.FindByEmailAsync(email);
    }

    private async Task<IList<string>> GetRolesAsync(User user)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        return await userManager.GetRolesAsync(user);
    }
}
