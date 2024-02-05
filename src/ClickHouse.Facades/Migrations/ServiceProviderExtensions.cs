using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades.Migrations;

public static class ServiceProviderExtensions
{
	public static Task ClickHouseMigrateAsync(this IServiceProvider serviceProvider)
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator>();

		return migrator.ApplyMigrationsAsync();
	}

	public static Task ClickHouseMigrateAsync<TContext>(this IServiceProvider serviceProvider)
		where TContext : ClickHouseContext<TContext>
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator<TContext>>();

		return migrator.ApplyMigrationsAsync();
	}

	public static Task ClickHouseRollbackAsync(this IServiceProvider serviceProvider, ulong targetMigrationId)
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator>();

		return migrator.RollbackAsync(targetMigrationId);
	}

	public static Task ClickHouseRollbackAsync<TContext>(
		this IServiceProvider serviceProvider,
		ulong targetMigrationId)
		where TContext : ClickHouseContext<TContext>
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator<TContext>>();

		return migrator.RollbackAsync(targetMigrationId);
	}
}
