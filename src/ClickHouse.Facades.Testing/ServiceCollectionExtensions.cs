using ClickHouse.Facades.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades.Testing;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddClickHouseTestContext<TContext, TContextFactory>(
		this IServiceCollection services,
		Action<ClickHouseContextServiceBuilder<TContext>> builderAction,
		ServiceLifetime factoryLifetime = ServiceLifetime.Singleton,
		Type? exposedFactoryType = null)
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
					(_, _) => new ClickHouseConnectionBrokerStub<TContext>(serviceProvider)),
			factoryLifetime);

		if (exposedFactoryType != null)
		{
			if (!typeof(TContextFactory).IsAssignableFrom(exposedFactoryType))
			{
				throw new InvalidOperationException(
					$"The type '{exposedFactoryType.FullName}' cannot be cast to '{typeof(TContextFactory).FullName}'. " +
					"Ensure that the specified type is compatible with the context factory type.");
			}

			var typedDescriptor = new ServiceDescriptor(
				typeof(TContextFactory),
				serviceProvider => serviceProvider.GetRequiredService<IClickHouseContextFactory<TContext>>(),
				factoryLifetime);

			services.Add(typedDescriptor);
		}

		services.Add(descriptor);

		var builder = ClickHouseContextServiceBuilder<TContext>.Create;
		builderAction(builder);
		builder.Build(services);

		services.AddSingleton<ClickHouseFacadeFactory<TContext>>();
		services.AddSingleton<ClickHouseConnectionTracker<TContext>>();
		services.AddSingleton<ClickHouseConnectionResponseProducer<TContext>>();

		return services;
	}
}
