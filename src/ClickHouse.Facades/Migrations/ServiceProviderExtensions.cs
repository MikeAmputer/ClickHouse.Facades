using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades.Migrations;

public static class ServiceProviderExtensions
{
	public static async Task<IClickHouseMigrationLog> ClickHouseMigrateAsync(this IServiceProvider serviceProvider)
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator>();

		await migrator.ApplyMigrationsAsync();

		return migrator.MigrationLog;
	}

	public static async Task<IClickHouseMigrationLog> ClickHouseMigrateAsync<TContext>(
		this IServiceProvider serviceProvider)
		where TContext : ClickHouseContext<TContext>
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator<TContext>>();

		await migrator.ApplyMigrationsAsync();

		return migrator.MigrationLog;
	}

	public static async Task<IClickHouseMigrationLog> ClickHouseRollbackAsync(
		this IServiceProvider serviceProvider,
		ulong targetMigrationId)
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator>();

		await migrator.RollbackAsync(targetMigrationId);

		return migrator.MigrationLog;
	}

	public static async Task<IClickHouseMigrationLog> ClickHouseRollbackAsync<TContext>(
		this IServiceProvider serviceProvider,
		ulong targetMigrationId)
		where TContext : ClickHouseContext<TContext>
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator<TContext>>();

		await migrator.RollbackAsync(targetMigrationId);

		return migrator.MigrationLog;
	}
}
