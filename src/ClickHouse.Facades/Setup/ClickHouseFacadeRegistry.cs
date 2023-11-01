using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades;

internal sealed class ClickHouseFacadeRegistry<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private readonly HashSet<Type> _facades = new();
	private readonly Dictionary<Type, Type> _facadeAbstractions = new();

	internal void AddFacade<TFacade>()
		where TFacade : ClickHouseFacade<TContext>
	{
		if (!_facades.Add(typeof(TFacade)))
		{
			throw new InvalidOperationException($"Facade of type {typeof(TFacade)} is already registered.");
		}
	}

	internal void AddFacade<TAbstraction, TFacade>()
		where TFacade : ClickHouseFacade<TContext>, TAbstraction
		where TAbstraction : class
	{
		var implementationType = typeof(TFacade);
		var serviceType = typeof(TAbstraction);

		if (!_facadeAbstractions.TryAdd(serviceType, implementationType))
		{
			throw new InvalidOperationException(
				$"Facade of abstraction type {serviceType} is already registered.");
		}
	}

	internal bool Contains<TFacade>()
		where TFacade : ClickHouseFacade<TContext>
	{
		return _facades.Contains(typeof(TFacade));
	}

	internal bool ContainsAbstraction<TAbstraction>()
		where TAbstraction : class
	{
		return _facadeAbstractions.ContainsKey(typeof(TAbstraction));
	}

	internal void RegisterFacades(IServiceCollection serviceCollection)
	{
		foreach (var facade in _facades)
		{
			serviceCollection.AddTransient(facade);
		}

		foreach (var (serviceType, implementationType) in _facadeAbstractions)
		{
			serviceCollection.AddTransient(serviceType, implementationType);
		}
	}
}
