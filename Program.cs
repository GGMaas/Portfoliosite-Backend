using Portfoliosite_Backend.Data;
using Portfoliosite_Backend.Services;
using Microsoft.EntityFrameworkCore;
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Entities.Identity;
using Microsoft.AspNetCore.Identity;

// This is the main startup file for the ASP.NET Core API.
// It configures the app, registers services, and starts the web server.

// Create the application builder.
// This is where we add services, configuration, and middleware for the API.
var builder = WebApplication.CreateBuilder(args);

// Connect the app to SQL Server using the connection string named "DefaultConnection"
// from appsettings.json or appsettings.Development.json.
// AppDbContext is the Entity Framework Core database context for the project.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Allow requests from the frontend application.
// This is important when the frontend runs on a different URL/port than the backend.
builder.Services.AddCors(options =>
{
    // Define a CORS policy called "AllowFrontend".
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register MVC controllers so the app can receive HTTP requests and route them
// to the correct controller actions.
builder.Services.AddControllers();

// Add API explorer support and Swagger generation.
// Swagger creates a web page that shows the available API endpoints and lets you test them.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the CRUD services with dependency injection.
// This means the controllers can ask for a BlogpostService or ProjectService
// without creating them manually.
builder.Services.AddScoped<ICrudService<BlogpostResponseDto, CreateBlogpostDto>, BlogpostService>();
builder.Services.AddScoped<ICrudService<ProjectResponseDto, CreateProjectDto>, ProjectService>();


// Add Authentication and Authorization to User with Framwork
builder.Services.AddIdentityApiEndpoints<User>()
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();


builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
.AddPolicy("CanEditProjects", policy => policy.RequireRole("Editor", "Admin"));



// Build the application after all services are configured.
var app = builder.Build();

// In development mode, enable Swagger UI so developers can test the API in the browser.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API v1");
        c.RoutePrefix = string.Empty;

        // Show the "Try it out" feature by default in Swagger UI.
        c.EnableTryItOutByDefault();
    });
}

// Redirect HTTP requests to HTTPS for safer connections.
app.UseHttpsRedirection();

// This app does not currently use authentication, but this middleware is usually
// used when authorization rules are configured.
app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityApi<User>();

// Use the configured CORS policy before mapping controllers.
app.UseCors("AllowFrontend");

// Map controller routes such as /Blogposts and /Projects.
app.MapControllers();

// Start the API and keep it running.
app.Run();