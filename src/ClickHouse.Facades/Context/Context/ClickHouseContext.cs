using ClickHouse.Driver;
using ClickHouse.Driver.ADO;

namespace ClickHouse.Facades;

public abstract class ClickHouseContext<TContext> : IAsyncDisposable
	where TContext : ClickHouseContext<TContext>
{
	private bool _initialized = false;
	private ClickHouseClient? _client = null;
	private readonly QueryOptionsBuilder _queryOptionsBuilder = new();
	private ClickHouseConnection? _connection = null;
	private IClickHouseConnectionBroker _connectionBroker = null!;
	private TransactionBroker _transactionBroker = null!;

	private ClickHouseFacadeFactory<TContext> _facadeFactory = null!;
	private readonly Dictionary<Type, object> _facades = new();

	private bool _allowDatabaseChanges = false;

	public string Database
	{
		get
		{
			ThrowIfNotInitialized();

			return _connection!.Database;
		}
	}

	public TFacade GetFacade<TFacade>()
		where TFacade : ClickHouseFacade<TContext>
	{
		ThrowIfNotInitialized();

		if (_facades.TryGetValue(typeof(TFacade), out var facade))
		{
			return (TFacade) facade;
		}

		var newFacade = _facadeFactory.CreateFacade<TFacade>(_connectionBroker);
		_facades.Add(typeof(TFacade), newFacade);

		return newFacade;
	}

	public TAbstraction GetFacadeAbstraction<TAbstraction>()
		where TAbstraction : class
	{
		ThrowIfNotInitialized();

		if (_facades.TryGetValue(typeof(TAbstraction), out var abstraction))
		{
			return (TAbstraction) abstraction;
		}

		var newFacade = _facadeFactory.CreateFacadeAbstraction<TAbstraction>(_connectionBroker);
		_facades.Add(typeof(TAbstraction), newFacade);

		return newFacade;
	}

	public void ChangeDatabase(string databaseName)
	{
		ThrowIfNotInitialized();

		if (!_allowDatabaseChanges)
		{
			throw new InvalidOperationException("Database changes are not allowed.");
		}

		_connection!.ChangeDatabase(databaseName);
		_queryOptionsBuilder.WithDatabase(databaseName);
	}

	public async Task SetSessionParameterAsync(string parameterName, object value)
	{
		ThrowIfNotInitialized();

		_queryOptionsBuilder.AddCustomSettings(parameterName, value);
		await _connectionBroker.SetSessionParameterAsync(parameterName, value);
	}

	public Task BeginTransactionAsync()
	{
		ThrowIfNotInitialized();

		return _transactionBroker.BeginAsync();
	}

	public Task CommitTransactionAsync()
	{
		ThrowIfNotInitialized();

		return _transactionBroker.CommitAsync();
	}

	public Task RollbackTransactionAsync()
	{
		ThrowIfNotInitialized();

		return _transactionBroker.RollbackAsync();
	}

	internal async Task Initialize(ClickHouseContextOptions<TContext> options)
	{
		ThrowIfInitialized();

		_client = CreateClient(options);
		_connection = _client!.CreateConnection();

		_connectionBroker = options.ConnectionBrokerProvider(new ConnectionBrokerParameters
		{
			Client = _client,
			QueryOptionsBuilder = _queryOptionsBuilder,
			Connection = _connection,
			CommandExecutionStrategy = ICommandExecutionStrategy.Pick(options.CommandExecutionStrategy),
			CommandExecutionListener = options.CommandExecutionListener,
		});

		_facadeFactory = options.FacadeFactory;
		_allowDatabaseChanges = options.AllowDatabaseChanges;

		_transactionBroker = await TransactionBroker.Create(_connectionBroker, options.TransactionBrokerOptions);

		_initialized = true;
	}

	private static ClickHouseClient CreateClient(ClickHouseContextOptions<TContext> options)
	{
		var clientSettings = new ClickHouseClientSettings(options.ConnectionString)
		{
			HttpClient = options.HttpClient,
			HttpClientFactory = options.HttpClientFactory,
			HttpClientName = options.HttpClientName,
			UseFormDataParameters = options.ParametersInBody,
			CustomSettings = options.ConnectionCustomSettings ?? new Dictionary<string, object>(),
		};

		return new ClickHouseClient(clientSettings);
	}

	private void ThrowIfNotInitialized()
	{
		if (!_initialized)
		{
			throw new InvalidOperationException("ClickHouse context is not initialized.");
		}
	}

	private void ThrowIfInitialized()
	{
		if (_initialized)
		{
			throw new InvalidOperationException("ClickHouse context is initialized.");
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_connection != null)
		{
			// might use _connection while disposing
			await _transactionBroker.DisposeAsync();

			// disposes owned client
			await _connection.DisposeAsync().ConfigureAwait(false);
		}
	}
}
