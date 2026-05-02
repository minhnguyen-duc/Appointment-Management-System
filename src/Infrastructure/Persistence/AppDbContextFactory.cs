using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

/// <summary>
/// Allows 'dotnet ef migrations add' and 'dotnet ef database update' to run
/// at design time without needing the full DI container to be available.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=US-PF3M9SRR\\SQLEXPRESS;Database=AppointmentAMS;" +
                "User Id=sa;Password=Minh@1003;" +
                "TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=True;",
                sql => sql.MigrationsAssembly("Infrastructure"))
            .Options;

        return new AppDbContext(opts);
    }
}
