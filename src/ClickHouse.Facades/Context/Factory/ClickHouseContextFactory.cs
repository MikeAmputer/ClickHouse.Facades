﻿namespace ClickHouse.Facades;

public abstract class ClickHouseContextFactory<TContext> : IClickHouseContextFactory<TContext>
	where TContext : ClickHouseContext<TContext>, new()
{
	private ClickHouseFacadeFactory<TContext> _facadeFactory = null!;

	private Func<ConnectionBrokerParameters, IClickHouseConnectionBroker>
		_connectionBrokerProvider = null!;

	internal ClickHouseContextFactory<TContext> Setup(
		ClickHouseFacadeFactory<TContext> facadeFactory,
		Func<ConnectionBrokerParameters, IClickHouseConnectionBroker> connectionBrokerProvider)
	{
		_facadeFactory = facadeFactory ?? throw new ArgumentNullException(nameof(facadeFactory));
		_connectionBrokerProvider = connectionBrokerProvider
			?? throw new ArgumentNullException(nameof(connectionBrokerProvider));

		return this;
	}

	public async Task<TContext> CreateContextAsync()
	{
		var builder = ClickHouseContextOptionsBuilder<TContext>.Create;

		SetupContextOptions(builder);

		var contextOptions = builder
			.WithFacadeFactory(_facadeFactory)
			.WithConnectionBrokerProvider(_connectionBrokerProvider)
			.Build();

		var context = new TContext();
		await context.Initialize(contextOptions);

		return context;
	}

	public IClickHouseRetryableExecutor<TContext> CreateRetryableExecutor()
	{
		return new ClickHouseRetryableExecutor<TContext>(this, DefaultRetryPolicy);
	}

	protected virtual ClickHouseRetryPolicy DefaultRetryPolicy => new();

	protected abstract void SetupContextOptions(ClickHouseContextOptionsBuilder<TContext> optionsBuilder);
}
