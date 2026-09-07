using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace Api.Instartups.Auth.Configurations.Database;


public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {

    }

    public DbSet<UserSessionsModel> UserSessions => Set<UserSessionsModel>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly,
            t => t.IsAssignableTo(typeof(IAppConfiguration)));
    }
}