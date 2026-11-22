using System.Net;
using System.Net.Http.Json;
using Portfoliosite_Backend.Data.Database_entities;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Entities.Identity;
using PortfolioSite.Tests.Infrastructure;

namespace PortfolioSite.Tests.Projects;

public sealed class ProjectsCrudTests : CrudApiTestBase
{
    private const string Route = "/api/Projects";

    [Fact]
    public async Task GetAll_returns_projects_without_authentication()
    {
        // Arrange
        var id = await AddProjectAsync(CreateProject("Portfolio", "Original"));

        // Act
        var response = await Client.GetAsync(Route);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectResponseDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(projects);
        Assert.Contains(projects, project => project.Id == id && project.Name == "Portfolio");
    }

    [Fact]
    public async Task GetById_returns_project_when_it_exists()
    {
        // Arrange
        var id = await AddProjectAsync(CreateProject("Portfolio", "Original"));

        // Act
        var response = await Client.GetAsync($"{Route}/{id}");
        var project = await response.Content.ReadFromJsonAsync<ProjectResponseDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(project);
        Assert.Equal("Portfolio", project.Name);
        Assert.Equal("Original", project.Description);
    }

    [Fact]
    public async Task GetById_returns_not_found_when_project_is_missing()
    {
        // Arrange
        const int missingId = 999;

        // Act
        var response = await Client.GetAsync($"{Route}/{missingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_persists_project_and_returns_created_response_for_admin()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);
        var payload = new CreateProjectDto
        {
            Name = "Portfolio",
            Description = "A project",
            img = "portfolio.png",
            link = "https://example.test",
            year = 2026
        };

        // Act
        var response = await Client.PostAsJsonAsync(Route, payload);
        var created = await response.Content.ReadFromJsonAsync<ProjectResponseDto>();
        var persisted = created is null ? null : await FindProjectAsync(created.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.NotNull(persisted);
        Assert.True(created.Id > 0);
        Assert.Equal(payload.Name, created.Name);
        Assert.Equal(payload.Description, created.Description);
        Assert.Equal(payload.img, created.img);
        Assert.Equal(payload.link, created.link);
        Assert.Equal(payload.year, created.year);
        Assert.Equal(payload.Name, persisted.Name);
    }

    [Fact]
    public async Task Editor_can_create_project()
    {
        // Arrange
        AuthenticateAs(UserRoles.Editor);
        var payload = new CreateProjectDto { Name = "Editor project" };

        // Act
        var response = await Client.PostAsJsonAsync(Route, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_changes_existing_project()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);
        var id = await AddProjectAsync(CreateProject("Before", "Old description"));
        var payload = new CreateProjectDto
        {
            Name = "After",
            Description = "New description",
            img = "new.png",
            link = "https://updated.example.test",
            year = 2025
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{Route}/{id}", payload);
        var updated = await FindProjectAsync(id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(payload.Name, updated.Name);
        Assert.Equal(payload.Description, updated.Description);
        Assert.Equal(payload.img, updated.img);
        Assert.Equal(payload.link, updated.link);
        Assert.Equal(payload.year, updated.year);
    }

    [Fact]
    public async Task Update_returns_not_found_when_project_is_missing()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);
        var payload = new CreateProjectDto { Name = "Missing project" };

        // Act
        var response = await Client.PutAsJsonAsync($"{Route}/999", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_existing_project()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);
        var id = await AddProjectAsync(CreateProject("To delete", "Temporary"));

        // Act
        var response = await Client.DeleteAsync($"{Route}/{id}");
        var deleted = await FindProjectAsync(id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Delete_returns_not_found_when_project_is_missing()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);

        // Act
        var response = await Client.DeleteAsync($"{Route}/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/Projects")]
    [InlineData("PUT", "/api/Projects/999")]
    [InlineData("DELETE", "/api/Projects/999")]
    public async Task Write_routes_reject_anonymous_requests(string method, string path)
    {
        // Arrange
        using var request = CreateWriteRequest(method, path, new CreateProjectDto());

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/Projects")]
    [InlineData("PUT", "/api/Projects/999")]
    [InlineData("DELETE", "/api/Projects/999")]
    public async Task Write_routes_reject_users_without_edit_role(string method, string path)
    {
        // Arrange
        AuthenticateAs(UserRoles.Gast);
        using var request = CreateWriteRequest(method, path, new CreateProjectDto());

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static Project CreateProject(string name, string description)
    {
        return new Project
        {
            Name = name,
            Description = description,
            img = "project.png",
            link = "https://example.test",
            year = 2024
        };
    }
}
