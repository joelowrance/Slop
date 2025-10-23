using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace VerdaVida.Shared.EntityFrameworkExtensions;

public static partial class MigrateDbContextExtensions
{
    private class MigrationHostedService<TContext>(IServiceProvider serviceProvider, Func<TContext, IServiceProvider, Task> seeder)
		: BackgroundService where TContext : DbContext
	{
		public override Task StartAsync(CancellationToken cancellationToken)
		{
			return serviceProvider.MigrateDbContextAsync(seeder);
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return Task.CompletedTask;
		}
	}


}
