using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades.Migrations;

public static class ServiceProviderExtensions
{
	public static Task ClickHouseMigrateAsync(this IServiceProvider serviceProvider)
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator>();

		return migrator.ApplyMigrationsAsync();
	}

	public static Task ClickHouseRollbackAsync(this IServiceProvider serviceProvider, ulong targetMigrationId)
	{
		var migrator = serviceProvider.GetRequiredService<IClickHouseMigrator>();

		return migrator.RollbackAsync(targetMigrationId);
	}
}
