namespace ClickHouse.Facades;

public abstract class ClickHouseContextFactory<TContext> : IClickHouseContextFactory<TContext>
	where TContext : ClickHouseContext<TContext>, new()
{
	private ClickHouseFacadeRegistry<TContext>? _facadeRegistry;

	internal ClickHouseContextFactory<TContext> Setup(ClickHouseFacadeRegistry<TContext> facadeRegistry)
	{
		_facadeRegistry = facadeRegistry ?? throw new ArgumentNullException(nameof(facadeRegistry));

		return this;
	}

	public TContext CreateContext()
	{
		if (_facadeRegistry == null)
		{
			throw new InvalidOperationException($"{GetType()} has no facade registry.");
		}

		var builder = ClickHouseContextOptionsBuilder<TContext>.Create;

		SetupContextOptions(builder);

		var contextOptions = builder
			.WithFacadeRegistry(_facadeRegistry)
			.Build();

		var context = new TContext();
		context.Initialize(contextOptions);

		return context;
	}

	protected abstract void SetupContextOptions(ClickHouseContextOptionsBuilder<TContext> optionsBuilder);
}
