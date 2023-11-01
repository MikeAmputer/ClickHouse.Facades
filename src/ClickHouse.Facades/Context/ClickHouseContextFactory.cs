using ClickHouse.Client.ADO;

namespace ClickHouse.Facades;

public abstract class ClickHouseContextFactory<TContext> : IClickHouseContextFactory<TContext>
	where TContext : ClickHouseContext<TContext>, new()
{
	private ClickHouseFacadeFactory<TContext> _facadeFactory = null!;
	private Func<ClickHouseConnection, ClickHouseConnectionBroker> _connectionBrokerProvider = null!;

	internal ClickHouseContextFactory<TContext> Setup(
		ClickHouseFacadeFactory<TContext> facadeFactory,
		Func<ClickHouseConnection, ClickHouseConnectionBroker> connectionBrokerProvider)
	{
		_facadeFactory = facadeFactory ?? throw new ArgumentNullException(nameof(facadeFactory));
		_connectionBrokerProvider = connectionBrokerProvider
			?? throw new ArgumentNullException(nameof(connectionBrokerProvider));

		return this;
	}

	public TContext CreateContext()
	{
		var builder = ClickHouseContextOptionsBuilder<TContext>.Create;

		SetupContextOptions(builder);

		var contextOptions = builder
			.WithFacadeFactory(_facadeFactory)
			.WithConnectionBrokerProvider(_connectionBrokerProvider)
			.Build();

		var context = new TContext();
		context.Initialize(contextOptions);

		return context;
	}

	protected abstract void SetupContextOptions(ClickHouseContextOptionsBuilder<TContext> optionsBuilder);
}
