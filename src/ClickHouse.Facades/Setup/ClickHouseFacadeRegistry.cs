using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades;

internal sealed class ClickHouseFacadeRegistry<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private readonly List<Type> _facades = new();

	internal void AddFacade<TFacade>()
		where TFacade : ClickHouseFacade<TContext>
	{
		_facades.Add(typeof(TFacade));
	}

	internal bool Contains<TFacade>()
		where TFacade : ClickHouseFacade<TContext>
	{
		return _facades.Contains(typeof(TFacade));
	}

	internal void RegisterFacades(IServiceCollection serviceCollection)
	{
		foreach (var facade in _facades)
		{
			serviceCollection.AddTransient(facade);
		}
	}
}
