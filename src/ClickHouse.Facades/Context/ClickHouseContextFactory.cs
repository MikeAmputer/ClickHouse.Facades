namespace ClickHouse.Facades;

public abstract class ClickHouseContextFactory<TContext> : IClickHouseContextFactory<TContext>
	where TContext : ClickHouseContext<TContext>, new()
{
	private ClickHouseFacadeFactory<TContext>? _facadeFactory;

	internal ClickHouseContextFactory<TContext> Setup(ClickHouseFacadeFactory<TContext> facadeFactory)
	{
		_facadeFactory = facadeFactory ?? throw new ArgumentNullException(nameof(facadeFactory));

		return this;
	}

	public TContext CreateContext()
	{
		if (_facadeFactory == null)
		{
			throw new InvalidOperationException($"{GetType()} has no facade registry.");
		}

		var builder = ClickHouseContextOptionsBuilder<TContext>.Create;

		SetupContextOptions(builder);

		var contextOptions = builder
			.WithFacadeFactory(_facadeFactory)
			.Build();

		var context = new TContext();
		context.Initialize(contextOptions);

		return context;
	}

	protected abstract void SetupContextOptions(ClickHouseContextOptionsBuilder<TContext> optionsBuilder);
}
