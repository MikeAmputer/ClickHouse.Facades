﻿using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

public sealed class ClickHouseContextOptionsBuilder<TContext>
	: Builder<ClickHouseContextOptions<TContext>, ClickHouseContextOptionsBuilder<TContext>>
	where TContext : ClickHouseContext<TContext>
{
	private const string UseSessionParameterName = "UseSession";

	private OptionalValue<string> _connectionString;
	private OptionalValue<bool> _forceSession;
	private OptionalValue<bool> _allowDatabaseChanges;

	private OptionalValue<HttpClient> _httpClient;
	private OptionalValue<IHttpClientFactory> _httpClientFactory;
	private OptionalValue<string> _httpClientName;

	private OptionalValue<ClickHouseFacadeFactory<TContext>> _facadeFactory;

	private OptionalValue<
		Func<ConnectionBrokerParameters, IClickHouseConnectionBroker>> _connectionBrokerProvider;

	private OptionalValue<CommandExecutionStrategy> _commandExecutionStrategy;
	private OptionalValue<IClickHouseCommandExecutionListener> _commandExecutionListener;

	private OptionalValue<Action<TransactionBrokerOptionsBuilder>> _setupTransactionBrokerOptions;

	private OptionalValue<IDictionary<string, object>> _connectionCustomSettings;
	private OptionalValue<bool> _parametersInBody;

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
		ExceptionHelpers.ThrowIfNull(httpClientFactory);
		ExceptionHelpers.ThrowIfNull(httpClientName);

		if (_httpClient.HasValue)
		{
			throw new InvalidOperationException(
				"Unable to set IHttpClientFactory and HttpClient simultaneously.");
		}

		WithPropertyValue(
			builder => builder._httpClientName,
			(builder, value) => builder._httpClientName = value,
			httpClientName);

		return WithPropertyValue(
			builder => builder._httpClientFactory,
			(builder, value) => builder._httpClientFactory = value,
			httpClientFactory);
	}

	public ClickHouseContextOptionsBuilder<TContext> WithHttpClient(HttpClient httpClient)
	{
		ExceptionHelpers.ThrowIfNull(httpClient);

		if (_httpClientFactory.HasValue)
		{
			throw new InvalidOperationException(
				"Unable to set IHttpClientFactory and HttpClient simultaneously.");
		}

		return WithPropertyValue(
			builder => builder._httpClient,
			(builder, value) => builder._httpClient = value,
			httpClient);
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
		return WithPropertyValue(
			builder => builder._forceSession,
			(builder, value) => builder._forceSession = value,
			true);
	}

	public ClickHouseContextOptionsBuilder<TContext> WithConnectionString(string connectionString)
	{
		if (connectionString.IsNullOrWhiteSpace())
		{
			throw new ArgumentException($"{nameof(connectionString)} is null or whitespace.");
		}

		return WithPropertyValue(
			builder => builder._connectionString,
			(builder, value) => builder._connectionString = value,
			connectionString);
	}

	public ClickHouseContextOptionsBuilder<TContext> WithConnectionCustomSettings(
		IDictionary<string, object> customSettings)
	{
		return WithPropertyValue(
			builder => builder._connectionCustomSettings,
			(builder, value) => builder._connectionCustomSettings = value,
			customSettings);
	}

	public ClickHouseContextOptionsBuilder<TContext> SendParametersInBody()
	{
		return WithPropertyValue(
			builder => builder._parametersInBody,
			(builder, value) => builder._parametersInBody = value,
			true);
	}

	internal ClickHouseContextOptionsBuilder<TContext> WithFacadeFactory(
		ClickHouseFacadeFactory<TContext> facadeFactory)
	{
		ExceptionHelpers.ThrowIfNull(facadeFactory);

		return WithPropertyValue(
			builder => builder._facadeFactory,
			(builder, value) => builder._facadeFactory = value,
			facadeFactory);
	}

	internal ClickHouseContextOptionsBuilder<TContext> WithConnectionBrokerProvider(
		Func<ConnectionBrokerParameters, IClickHouseConnectionBroker> connectionBrokerProvider)
	{
		ExceptionHelpers.ThrowIfNull(connectionBrokerProvider);

		return WithPropertyValue(
			builder => builder._connectionBrokerProvider,
			(builder, value) => builder._connectionBrokerProvider = value,
			connectionBrokerProvider);
	}

	protected override ClickHouseContextOptions<TContext> BuildCore()
	{
		var connectionString = _connectionString.NotNullOrThrow();

		if (_forceSession.OrElseValue(false))
		{
			connectionString = GetSessionConnectionString(connectionString);
		}

		var transactionBrokerOptionsBuilder = TransactionBrokerOptionsBuilder.Create;
		_setupTransactionBrokerOptions.OrDefault()?.Invoke(transactionBrokerOptionsBuilder);

		var transactionBrokerOptions = transactionBrokerOptionsBuilder.Build();

		return new ClickHouseContextOptions<TContext>
		{
			ConnectionString = connectionString,
			AllowDatabaseChanges = _allowDatabaseChanges.OrElseValue(false),
			HttpClient = _httpClient.OrDefault(),
			HttpClientFactory = _httpClientFactory.OrDefault(),
			HttpClientName = _httpClientName.OrDefault(),
			FacadeFactory = _facadeFactory.NotNullOrThrow(),
			ConnectionBrokerProvider = _connectionBrokerProvider.NotNullOrThrow(),
			CommandExecutionStrategy = _commandExecutionStrategy.OrElseValue(CommandExecutionStrategy.Default),
			CommandExecutionListener = _commandExecutionListener.OrElseValue(null),
			TransactionBrokerOptions = transactionBrokerOptions,
			ConnectionCustomSettings = _connectionCustomSettings.OrElseValue(null),
			ParametersInBody = _parametersInBody.OrDefault(),
		};
	}

	private static string GetSessionConnectionString(string connectionString)
	{
		var connectionParameters = connectionString.GetConnectionStringParameters();

		var parameterExists = connectionParameters.TryGetValue(UseSessionParameterName.ToLower(), out var value);

		if (!parameterExists)
		{
			return $"{connectionString.TrimEnd(';')};{UseSessionParameterName}=True;";
		}

		if (value == null || !bool.Parse(value))
		{
			return connectionString.Replace(
				$"{UseSessionParameterName}=False",
				$"{UseSessionParameterName}=True",
				StringComparison.InvariantCultureIgnoreCase);
		}

		return connectionString;
	}
}
