using ClickHouse.Client.ADO;

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

	internal TFacade CreateFacade<TFacade>(ClickHouseConnection connection)
		where TFacade : ClickHouseFacade<TContext>, new()
	{
		if (_facades.Contains(typeof(TFacade)))
		{
			return (new TFacade().SetConnection(connection) as TFacade)!;
		}

		throw new InvalidOperationException($"Facade of type {typeof(TFacade)} was not found.");
	}
}
