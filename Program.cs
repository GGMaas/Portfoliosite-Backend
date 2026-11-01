using Portfoliosite_Backend.Data.Database_entities;
using Portfoliosite_Backend.Services;
using Microsoft.EntityFrameworkCore;
using Portfoliosite_Backend.DTOs;
using Microsoft.AspNetCore.Identity;
using Portfoliosite_Backend.Entities.Identity;

// This file starts the API.
// It sets up the database, routes, and security.

// Create the app builder.
// This is the place where we add services and setup the app.
var builder = WebApplication.CreateBuilder(args);

// Connect the app to the SQL database.
// The connection string is in appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Allow the frontend to call this API.
// This is needed when the frontend runs on another port or domain.
builder.Services.AddCors(options =>
{
    // Create a simple policy called "AllowFrontend".
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add controller support so API routes can work.
builder.Services.AddControllers();

// Add Swagger so we can test endpoints in the browser.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the services used by the controllers.
// This lets the app reuse the same service code everywhere.
builder.Services.AddScoped<ICrudService<BlogpostResponseDto, CreateBlogpostDto>, BlogpostService>();
builder.Services.AddScoped<ICrudService<ProjectResponseDto, CreateProjectDto>, ProjectService>();

// Add login and user/role support.
builder.Services.AddIdentityApiEndpoints<User>()
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
.AddPolicy("CanEditProjects", policy => policy.RequireRole("Editor", "Admin"));

builder.Services.AddProblemDetails();

// Build the app after everything is configured.
var app = builder.Build();

app.UseExceptionHandler();

// In development, show the API docs in the browser.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API v1");
        c.RoutePrefix = string.Empty;

        // Show the test feature in Swagger.
        c.EnableTryItOutByDefault();
    });
}

// Force HTTPS for safer requests.
app.UseHttpsRedirection();

// Use authentication and authorization rules.
app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityApi<User>();

// Use CORS before handling requests.
app.UseCors("AllowFrontend");

// Map all controller routes.
app.MapControllers();

// Start the app and keep it running.
app.Run();