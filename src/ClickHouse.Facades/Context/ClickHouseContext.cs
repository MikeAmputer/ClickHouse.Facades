using ClickHouse.Client.ADO;

namespace ClickHouse.Facades;

public abstract class ClickHouseContext<TContext> : IDisposable, IAsyncDisposable
	where TContext : ClickHouseContext<TContext>
{
	private bool _initialized = false;
	private ClickHouseConnection? _connection = null;
	private ClickHouseConnectionBroker _connectionBroker = null!;
	private ClickHouseFacadeFactory<TContext> _facadeFactory = null!;
	private readonly Dictionary<Type, ClickHouseFacade<TContext>> _facades = new();

	private bool _allowDatabaseChanges = false;

	public string Database
	{
		get
		{
			ThrowIfNotInitialized();

			return _connection!.Database;
		}
	}

	public string? ServerVersion
	{
		get
		{
			ThrowIfNotInitialized();

			return _connectionBroker.ServerVersion;
		}
	}

	public string? ServerTimezone
	{
		get
		{
			ThrowIfNotInitialized();

			return _connectionBroker.ServerTimezone;
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

		var newFacade = _facadeFactory.CreateFacade<TFacade>(_connectionBroker!);
		_facades.Add(typeof(TFacade), newFacade);

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
	}

	internal void Initialize(ClickHouseContextOptions<TContext> options)
	{
		ThrowIfInitialized();

		_connection = CreateConnection(options);
		_connectionBroker = options.ConnectionBrokerProvider(_connection);
		_facadeFactory = options.FacadeFactory;
		_allowDatabaseChanges = options.AllowDatabaseChanges;

		_initialized = true;
	}

	private static ClickHouseConnection CreateConnection(ClickHouseContextOptions<TContext> options)
	{
		if (options.HttpClient != null)
		{
			return new ClickHouseConnection(options.ConnectionString, options.HttpClient);
		}

		if (options.HttpClientFactory != null)
		{
			return new ClickHouseConnection(
				options.ConnectionString,
				options.HttpClientFactory,
				options.HttpClientName);
		}

		return new ClickHouseConnection(options.ConnectionString);
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

	public void Dispose()
	{
		_connection?.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		if (_connection != null)
		{
			await _connection.DisposeAsync().ConfigureAwait(false);
		}
	}
}
