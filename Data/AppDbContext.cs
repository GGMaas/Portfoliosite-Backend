using Microsoft.EntityFrameworkCore;

namespace Portfoliosite_Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Blogpost> Blogposts => Set<Blogpost>();
}