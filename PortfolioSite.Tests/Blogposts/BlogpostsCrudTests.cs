using System.Net;
using System.Net.Http.Json;
using Portfoliosite_Backend.Data.Database_entities;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Entities.Identity;
using PortfolioSite.Tests.Infrastructure;

namespace PortfolioSite.Tests.Blogposts;

public sealed class BlogpostsCrudTests : CrudApiTestBase
{
    private const string Route = "/api/Blogposts";

    [Fact]
    public async Task GetAll_returns_blogposts_without_authentication()
    {
        // Arrange
        var id = await AddBlogpostAsync(CreateBlogpost("First post", "Content"));

        // Act
        var response = await Client.GetAsync(Route);
        var blogposts = await response.Content.ReadFromJsonAsync<List<BlogpostResponseDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(blogposts);
        Assert.Contains(blogposts, post => post.Id == id && post.Title == "First post");
    }

    [Fact]
    public async Task GetById_returns_blogpost_when_it_exists()
    {
        // Arrange
        var id = await AddBlogpostAsync(CreateBlogpost("First post", "Content"));

        // Act
        var response = await Client.GetAsync($"{Route}/{id}");
        var blogpost = await response.Content.ReadFromJsonAsync<BlogpostResponseDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(blogpost);
        Assert.Equal("First post", blogpost.Title);
        Assert.Equal("Content", blogpost.Description);
    }

    [Fact]
    public async Task GetById_returns_not_found_when_blogpost_is_missing()
    {
        // Arrange
        const int missingId = 999;

        // Act
        var response = await Client.GetAsync($"{Route}/{missingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_persists_blogpost_and_returns_created_response_for_admin()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);
        var payload = new CreateBlogpostDto
        {
            Title = "First post",
            Description = "Post content"
        };

        // Act
        var response = await Client.PostAsJsonAsync(Route, payload);
        var created = await response.Content.ReadFromJsonAsync<BlogpostResponseDto>();
        var persisted = created is null ? null : await FindBlogpostAsync(created.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.NotNull(persisted);
        Assert.True(created.Id > 0);
        Assert.Equal(payload.Title, created.Title);
        Assert.Equal(payload.Description, created.Description);
        Assert.NotEqual(default, created.Date);
        Assert.Equal(payload.Title, persisted.Title);
    }

    [Fact]
    public async Task Editor_can_create_blogpost()
    {
        // Arrange
        AuthenticateAs(UserRoles.Editor);
        var payload = new CreateBlogpostDto { Title = "Editor post" };

        // Act
        var response = await Client.PostAsJsonAsync(Route, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_changes_existing_blogpost()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);
        var id = await AddBlogpostAsync(CreateBlogpost("Before", "Old content"));
        var payload = new CreateBlogpostDto
        {
            Title = "After",
            Description = "Updated content"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{Route}/{id}", payload);
        var updated = await FindBlogpostAsync(id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(payload.Title, updated.Title);
        Assert.Equal(payload.Description, updated.Description);
    }

    [Fact]
    public async Task Update_returns_not_found_when_blogpost_is_missing()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);
        var payload = new CreateBlogpostDto { Title = "Missing post" };

        // Act
        var response = await Client.PutAsJsonAsync($"{Route}/999", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_existing_blogpost()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);
        var id = await AddBlogpostAsync(CreateBlogpost("To delete", "Temporary"));

        // Act
        var response = await Client.DeleteAsync($"{Route}/{id}");
        var deleted = await FindBlogpostAsync(id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Delete_returns_not_found_when_blogpost_is_missing()
    {
        // Arrange
        AuthenticateAs(UserRoles.Admin);

        // Act
        var response = await Client.DeleteAsync($"{Route}/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/Blogposts")]
    [InlineData("PUT", "/api/Blogposts/999")]
    [InlineData("DELETE", "/api/Blogposts/999")]
    public async Task Write_routes_reject_anonymous_requests(string method, string path)
    {
        // Arrange
        using var request = CreateWriteRequest(method, path, new CreateBlogpostDto());

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/Blogposts")]
    [InlineData("PUT", "/api/Blogposts/999")]
    [InlineData("DELETE", "/api/Blogposts/999")]
    public async Task Write_routes_reject_users_without_edit_role(string method, string path)
    {
        // Arrange
        AuthenticateAs(UserRoles.Gast);
        using var request = CreateWriteRequest(method, path, new CreateBlogpostDto());

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static Blogpost CreateBlogpost(string title, string description)
    {
        return new Blogpost
        {
            Title = title,
            Description = description,
            Date = new DateTime(2026, 1, 1)
        };
    }
}
