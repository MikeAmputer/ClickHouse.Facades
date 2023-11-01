using ClickHouse.Facades.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades;

public sealed class ClickHouseContextServiceBuilder<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private readonly ClickHouseFacadeRegistry<TContext> _facadeRegistry;

	internal static ClickHouseContextServiceBuilder<TContext> Create => new();

	private ClickHouseContextServiceBuilder()
	{
		_facadeRegistry = new ClickHouseFacadeRegistry<TContext>();
	}

	public ClickHouseContextServiceBuilder<TContext> AddFacade<TFacade>()
		where TFacade : ClickHouseFacade<TContext>
	{
		_facadeRegistry.AddFacade<TFacade>();

		return this;
	}

	public ClickHouseContextServiceBuilder<TContext> AddFacade<TAbstraction, TFacade>()
		where TFacade : ClickHouseFacade<TContext>, TAbstraction
		where TAbstraction : class
	{
		_facadeRegistry.AddFacade<TAbstraction, TFacade>();

		return this;
	}

	internal void Build(IServiceCollection serviceCollection)
	{
		ExceptionHelpers.ThrowIfNull(serviceCollection);

		serviceCollection.AddSingleton(_facadeRegistry);

		_facadeRegistry.RegisterFacades(serviceCollection);
	}
}
