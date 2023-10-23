using ClickHouse.Facades.Migrations;
using ClickHouse.Facades.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddClickHouseMigrations<TInstructions, TLocator>(
		this IServiceCollection services)
		where TInstructions : class, IClickHouseMigrationInstructions
		where TLocator : class, IClickHouseMigrationsLocator
	{
		return services
			.AddTransient<IClickHouseMigrationInstructions, TInstructions>()
			.AddTransient<IClickHouseMigrationsLocator, TLocator>()
			.AddClickHouseContext<ClickHouseMigrationContext, ClickHouseMigrationContextFactory>(
				builder => builder
					.AddFacade<ClickHouseMigrationFacade>(),
				ServiceLifetime.Transient)
			.AddTransient<IClickHouseMigrator, ClickHouseMigrator>();
	}

	public static IServiceCollection AddClickHouseContext<TContext, TContextFactory>(
		this IServiceCollection services,
		Action<ClickHouseContextServiceBuilder<TContext>> builderAction,
		ServiceLifetime factoryLifetime = ServiceLifetime.Singleton)
		where TContext : ClickHouseContext<TContext>, new()
		where TContextFactory : ClickHouseContextFactory<TContext>
	{
		ExceptionHelpers.ThrowIfNull(builderAction);

		var descriptor = new ServiceDescriptor(
			typeof(IClickHouseContextFactory<TContext>),
			serviceProvider => ActivatorUtilities
				.CreateInstance<TContextFactory>(serviceProvider)
				.Setup(serviceProvider.GetRequiredService<ClickHouseFacadeFactory<TContext>>()),
			factoryLifetime);

		services.Add(descriptor);

		var builder = ClickHouseContextServiceBuilder<TContext>.Create;
		builderAction(builder);
		builder.Build(services);

		services.AddTransient<ClickHouseFacadeFactory<TContext>>();

		return services;
	}
}
