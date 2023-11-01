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
			var facade = _serviceProvider.GetRequiredService<TFacade>();
			facade.SetConnectionBroker(connectionBroker);

			return facade;
		}

		throw new InvalidOperationException($"Facade of type {typeof(TFacade)} was not found.");
	}

	internal TAbstraction CreateFacadeAbstraction<TAbstraction>(ClickHouseConnectionBroker connectionBroker)
		where TAbstraction : class
	{
		if (_registry.ContainsAbstraction<TAbstraction>())
		{
			var abstraction = _serviceProvider.GetRequiredService<TAbstraction>();

			if (abstraction is ClickHouseFacade<TContext> facade)
			{
				facade.SetConnectionBroker(connectionBroker);
			}

			return abstraction;
		}

		throw new InvalidOperationException($"Facade abstraction of type {typeof(TAbstraction)} was not found.");
	}
}
