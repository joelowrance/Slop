using Microsoft.EntityFrameworkCore;

namespace VerdaVida.Shared.EntityFrameworkExtensions;

public interface IDbSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
