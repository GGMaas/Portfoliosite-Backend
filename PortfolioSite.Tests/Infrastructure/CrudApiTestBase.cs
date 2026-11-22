using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Portfoliosite_Backend.Data.Database_entities;
using Portfoliosite_Backend.Entities.Identity;

namespace PortfolioSite.Tests.Infrastructure;

public abstract class CrudApiTestBase : IAsyncLifetime
{
    protected PortfolioApiFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public Task InitializeAsync()
    {
        Factory = new PortfolioApiFactory();
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }

    protected void AuthenticateAs(string role)
    {
        Client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
    }

    protected async Task<int> AddProjectAsync(Project project)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return project.Id;
    }

    protected async Task<int> AddBlogpostAsync(Blogpost blogpost)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Blogposts.Add(blogpost);
        await context.SaveChangesAsync();
        return blogpost.Id;
    }

    protected async Task<Project?> FindProjectAsync(int id)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Projects.FindAsync(id);
    }

    protected async Task<Blogpost?> FindBlogpostAsync(int id)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Blogposts.FindAsync(id);
    }

    protected static HttpRequestMessage CreateWriteRequest(
        string method,
        string path,
        object payload)
    {
        return new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = System.Net.Http.Json.JsonContent.Create(payload)
        };
    }
}
