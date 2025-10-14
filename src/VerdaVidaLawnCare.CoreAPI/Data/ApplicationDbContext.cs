using Microsoft.EntityFrameworkCore;

namespace VerdaVidaLawnCare.CoreAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure any entities here
        // For now, we'll keep it minimal to verify the connection works
    }
}
