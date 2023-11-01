using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades;

internal class ClickHouseFacadeFactory<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private readonly ClickHouseFacadeRegistry<TContext> _registry;
	private readonly IServiceProvider _serviceProvider;

	public ClickHouseFacadeFactory(ClickHouseFacadeRegistry<TContext> registry, IServiceProvider serviceProvider)
	{
		_registry = registry ?? throw new ArgumentNullException(nameof(registry));
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	internal TFacade CreateFacade<TFacade>(ClickHouseConnectionBroker connectionBroker)
		where TFacade : ClickHouseFacade<TContext>
	{
		if (_registry.Contains<TFacade>())
		{
			return (_serviceProvider.GetRequiredService<TFacade>().SetConnectionBroker(connectionBroker) as TFacade)!;
		}

		throw new InvalidOperationException($"Facade of type {typeof(TFacade)} was not found.");
	}
}
