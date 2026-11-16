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
app.Run();

// Register the services the API needs before the app starts.
static void AddServices(WebApplicationBuilder builder)
{
    AddDatabase(builder);
    AddCors(builder.Services);
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
static void AddCors(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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
    services.AddIdentityApiEndpoints<User>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

    services.AddAuthentication();
    services.AddAuthorization();
    services.AddAuthorizationBuilder().AddPolicy("CanEditProjects", policy => policy.RequireRole("Editor", "Admin"));
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
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors("AllowFrontend");
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
