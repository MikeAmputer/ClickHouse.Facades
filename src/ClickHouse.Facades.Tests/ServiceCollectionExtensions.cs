using ClickHouse.Facades.Migrations;
using ClickHouse.Facades.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades.Tests;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddClickHouseTestMigrations<TInstructions, TLocator>(
		this IServiceCollection services,
		TInstructions instructions,
		TLocator locator)
		where TInstructions : class, IClickHouseMigrationInstructions
		where TLocator : class, IClickHouseMigrationsLocator
	{
		return services
			.AddSingleton<IClickHouseMigrationInstructions>(_ => instructions)
			.AddSingleton<IClickHouseMigrationsLocator>(_ => locator)
			.AddSingleton<IClickHouseMigrator, ClickHouseMigrator>()
			.AddClickHouseTestContext<ClickHouseMigrationContext, ClickHouseMigrationContextFactory>(
				builder => builder
					.AddFacade<IClickHouseMigrationFacade, ClickHouseMigrationFacade>());
	}
}
