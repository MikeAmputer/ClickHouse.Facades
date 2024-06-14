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
			.AddTransient<IClickHouseMigrator, ClickHouseMigrator>()
			.AddClickHouseContext<ClickHouseMigrationContext, ClickHouseMigrationContextFactory>(
				builder => builder
					.AddFacade<IClickHouseMigrationFacade, ClickHouseMigrationFacade>(),
				ServiceLifetime.Transient);
	}

	public static IServiceCollection AddClickHouseMigrations<TInstructions>(
		this IServiceCollection services)
		where TInstructions : class, IClickHouseMigrationInstructions
	{
		return services
			.AddTransient<IClickHouseMigrationInstructions, TInstructions>()
			.AddClickHouseContext<ClickHouseMigrationContext, ClickHouseMigrationContextFactory>(
				builder => builder
					.AddFacade<IClickHouseMigrationFacade, ClickHouseMigrationFacade>(),
				ServiceLifetime.Transient);
	}

	public static IServiceCollection AddClickHouseContextMigrations<TContext, TLocator>(
		this IServiceCollection services)
		where TContext : ClickHouseContext<TContext>
		where TLocator : class, IClickHouseMigrationsLocator<TContext>
	{
		return services
			.AddTransient<IClickHouseMigrationsLocator<TContext>, TLocator>()
			.AddTransient<IClickHouseMigrator<TContext>, ClickHouseContextMigrator<TContext>>();
	}

	public static IServiceCollection AddClickHouseContext<TContext, TContextFactory>(
		this IServiceCollection services,
		Action<ClickHouseContextServiceBuilder<TContext>> builderAction,
		ServiceLifetime factoryLifetime = ServiceLifetime.Singleton,
		bool exposeFactoryType = false)
		where TContext : ClickHouseContext<TContext>, new()
		where TContextFactory : ClickHouseContextFactory<TContext>
	{
		ExceptionHelpers.ThrowIfNull(builderAction);

		if (services.Any(service => service.ServiceType == typeof(IClickHouseContextFactory<TContext>)))
		{
			throw new InvalidOperationException(
				$"ClickHouse context of type {typeof(TContext)} is already registered.");
		}

		var descriptor = new ServiceDescriptor(
			typeof(IClickHouseContextFactory<TContext>),
			serviceProvider => ActivatorUtilities
				.CreateInstance<TContextFactory>(serviceProvider)
				.Setup(
					serviceProvider.GetRequiredService<ClickHouseFacadeFactory<TContext>>(),
					(connection, commandExecutionStrategy) => new ClickHouseConnectionBroker(
						connection,
						commandExecutionStrategy)),
			factoryLifetime);

		services.Add(descriptor);

		if (exposeFactoryType)
		{
			var typedDescriptor = new ServiceDescriptor(
				typeof(TContextFactory),
				serviceProvider => serviceProvider.GetRequiredService<IClickHouseContextFactory<TContext>>(),
				factoryLifetime);

			services.Add(typedDescriptor);
		}

		var builder = ClickHouseContextServiceBuilder<TContext>.Create;
		builderAction(builder);
		builder.Build(services);

		services.AddTransient<ClickHouseFacadeFactory<TContext>>();

		return services;
	}
}
