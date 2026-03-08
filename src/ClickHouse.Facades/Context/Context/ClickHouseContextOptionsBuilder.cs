using ClickHouse.Driver.ADO;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

public sealed class ClickHouseContextOptionsBuilder<TContext>
	: Builder<ClickHouseContextOptions<TContext>, ClickHouseContextOptionsBuilder<TContext>>
	where TContext : ClickHouseContext<TContext>
{
	private readonly ClickHouseClientSettingsBuilder _clientSettingsBuilder = new();

	private OptionalValue<bool> _allowDatabaseChanges;

	private OptionalValue<ClickHouseFacadeFactory<TContext>> _facadeFactory;

	private OptionalValue<
		Func<ConnectionBrokerParameters, IClickHouseConnectionBroker>> _connectionBrokerProvider;

	private OptionalValue<CommandExecutionStrategy> _commandExecutionStrategy;
	private OptionalValue<IClickHouseCommandExecutionListener> _commandExecutionListener;

	private OptionalValue<Action<TransactionBrokerOptionsBuilder>> _setupTransactionBrokerOptions;

	public ClickHouseContextOptionsBuilder<TContext> WithClientSettings(ClickHouseClientSettings settings)
	{
		_clientSettingsBuilder.WithClientSettings(settings);

		return this;
	}

	public ClickHouseContextOptionsBuilder<TContext> WithConnectionString(string connectionString)
	{
		_clientSettingsBuilder.WithConnectionString(connectionString);

		return this;
	}

	public ClickHouseContextOptionsBuilder<TContext> SetupTransactions(
		Action<TransactionBrokerOptionsBuilder> setup)
	{
		return WithPropertyValue(
			builder => builder._setupTransactionBrokerOptions,
			(builder, value) => builder._setupTransactionBrokerOptions = value,
			setup);
	}

	public ClickHouseContextOptionsBuilder<TContext> WithCommandExecutionStrategy(
		CommandExecutionStrategy commandExecutionStrategy)
	{
		return WithPropertyValue(
			builder => builder._commandExecutionStrategy,
			(builder, value) => builder._commandExecutionStrategy = value,
			commandExecutionStrategy);
	}

	public ClickHouseContextOptionsBuilder<TContext> WithCommandExecutionListener(
		IClickHouseCommandExecutionListener commandExecutionListener)
	{
		return WithPropertyValue(
			builder => builder._commandExecutionListener,
			(builder, value) => builder._commandExecutionListener = value,
			commandExecutionListener);
	}

	public ClickHouseContextOptionsBuilder<TContext> WithHttpClientFactory(
		IHttpClientFactory httpClientFactory,
		string httpClientName)
	{
		_clientSettingsBuilder.WithHttpClientFactory(httpClientFactory, httpClientName);

		return this;
	}

	public ClickHouseContextOptionsBuilder<TContext> WithHttpClient(HttpClient httpClient)
	{
		_clientSettingsBuilder.WithHttpClient(httpClient);

		return this;
	}

	public ClickHouseContextOptionsBuilder<TContext> AllowDatabaseChanges()
	{
		return WithPropertyValue(
			builder => builder._allowDatabaseChanges,
			(builder, value) => builder._allowDatabaseChanges = value,
			true);
	}

	public ClickHouseContextOptionsBuilder<TContext> ForceSessions()
	{
		_clientSettingsBuilder.UseSession();

		return this;
	}

	public ClickHouseContextOptionsBuilder<TContext> WithConnectionCustomSettings(
		IDictionary<string, object> customSettings)
	{
		_clientSettingsBuilder.WithCustomSettings(customSettings);

		return this;
	}

	public ClickHouseContextOptionsBuilder<TContext> SendParametersInBody()
	{
		_clientSettingsBuilder.UseFormDataParameters();

		return this;
	}

	internal ClickHouseContextOptionsBuilder<TContext> WithFacadeFactory(
		ClickHouseFacadeFactory<TContext> facadeFactory)
	{
		ArgumentNullException.ThrowIfNull(facadeFactory);

		return WithPropertyValue(
			builder => builder._facadeFactory,
			(builder, value) => builder._facadeFactory = value,
			facadeFactory);
	}

	internal ClickHouseContextOptionsBuilder<TContext> WithConnectionBrokerProvider(
		Func<ConnectionBrokerParameters, IClickHouseConnectionBroker> connectionBrokerProvider)
	{
		ArgumentNullException.ThrowIfNull(connectionBrokerProvider);

		return WithPropertyValue(
			builder => builder._connectionBrokerProvider,
			(builder, value) => builder._connectionBrokerProvider = value,
			connectionBrokerProvider);
	}

	protected override ClickHouseContextOptions<TContext> BuildCore()
	{
		var transactionBrokerOptionsBuilder = TransactionBrokerOptionsBuilder.Create;
		_setupTransactionBrokerOptions.OrDefault()?.Invoke(transactionBrokerOptionsBuilder);

		var transactionBrokerOptions = transactionBrokerOptionsBuilder.Build();

		return new ClickHouseContextOptions<TContext>
		{
			ClickHouseClientSettings = _clientSettingsBuilder.Build(),
			AllowDatabaseChanges = _allowDatabaseChanges.OrElseValue(false),
			FacadeFactory = _facadeFactory.NotNullOrThrow(),
			ConnectionBrokerProvider = _connectionBrokerProvider.NotNullOrThrow(),
			CommandExecutionStrategy = _commandExecutionStrategy.OrElseValue(CommandExecutionStrategy.Default),
			CommandExecutionListener = _commandExecutionListener.OrElseValue(null),
			TransactionBrokerOptions = transactionBrokerOptions,
		};
	}
}
