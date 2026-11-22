using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfoliosite_Backend.Data.Database_entities;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Entities.Identity;
using Portfoliosite_Backend.Services;

var builder = WebApplication.CreateBuilder(args);
AddServices(builder);
var app = builder.Build();
ConfigureRequestPipeline(app);
await EnsureRolesExistAsync(app.Services);
app.Run();

// Register the services the API needs before the app starts.
static void AddServices(WebApplicationBuilder builder)
{
    AddDatabase(builder);
    AddCors(builder);
    AddControllersAndSwagger(builder.Services);
    AddApplicationServices(builder.Services);
    AddIdentityAndAuthorization(builder.Services);
    AddErrorHandling(builder.Services);
}

// Connect the application to the SQL Server database.
static void AddDatabase(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Allow the frontend to send requests to this API.
static void AddCors(WebApplicationBuilder builder)
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.AllowAnyHeader().AllowAnyMethod();

            if (allowedOrigins.Length == 0)
            {
                policy.AllowAnyOrigin();
                return;
            }

            policy.WithOrigins(allowedOrigins).AllowCredentials();
        });
    });
}

// Enable controller routes and the Swagger API documentation.
static void AddControllersAndSwagger(IServiceCollection services)
{
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

// Make the application services available to controllers.
static void AddApplicationServices(IServiceCollection services)
{
    services.AddScoped<ICrudService<BlogpostResponseDto, CreateBlogpostDto>, BlogpostService>();
    services.AddScoped<ICrudService<ProjectResponseDto, CreateProjectDto>, ProjectService>();
    services.AddScoped<IUserRegistrationService, UserRegistrationService>();
}

// Enable user accounts, roles, authentication, and authorization policies.
static void AddIdentityAndAuthorization(IServiceCollection services)
{
    services.AddIdentity<User, IdentityRole>(options =>
    {
        var path = options.Password;
        path.RequireNonAlphanumeric = false;
        path.RequireDigit = false;
    })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

    services.Configure<SecurityStampValidatorOptions>(options =>
    {
        // Check for manual role changes on every request and invalidate stale cookies immediately.
        options.ValidationInterval = TimeSpan.Zero;
    });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("CanEditContent", policy =>
            policy.RequireRole(UserRoles.Editor, UserRoles.Admin));
    });
}

// Return clear error responses when a request fails.
static void AddErrorHandling(IServiceCollection services)
{
    services.AddProblemDetails();
}

// Add middleware and routes in the order requests should pass through them.
static void ConfigureRequestPipeline(WebApplication app)
{
    app.UseExceptionHandler();
    UseSwaggerInDevelopment(app);

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
}

// Show Swagger only while the application is running in development.
static void UseSwaggerInDevelopment(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        return;
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API v1");
        options.RoutePrefix = string.Empty;
        options.EnableTryItOutByDefault();
    });
}

// Create the roles used by registration and authorization if they do not exist yet.
static async Task EnsureRolesExistAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var roleName in new[] { UserRoles.Gast, UserRoles.Editor, UserRoles.Admin })
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            continue;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not create the '{roleName}' role: {errors}");
        }
    }
}

public partial class Program
{
}
