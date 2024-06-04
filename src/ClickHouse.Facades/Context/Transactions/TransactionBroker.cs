namespace ClickHouse.Facades;

internal sealed class TransactionBroker : IAsyncDisposable
{
	public static async Task<TransactionBroker> Create(
		IClickHouseConnectionBroker connectionBroker,
		TransactionBrokerOptions options)
	{
		var transactionBroker = new TransactionBroker(connectionBroker, options);

		if (options.AutoBeginTransaction)
		{
			await transactionBroker.BeginAsync();
		}

		return transactionBroker;
	}

	private TransactionState _state = TransactionState.NotStarted;

	private readonly IClickHouseConnectionBroker _connectionBroker;

	private readonly TransactionBrokerOptions _options;

	private TransactionBroker(IClickHouseConnectionBroker connectionBroker, TransactionBrokerOptions options)
	{
		_connectionBroker = connectionBroker ?? throw new ArgumentNullException(nameof(connectionBroker));
		_options = options ?? throw new ArgumentNullException(nameof(options));

		if (options is { AutoCommitTransaction: true, AutoRollbackTransaction: true })
		{
			throw new InvalidOperationException(
				"Ambiguous context transaction behavior. Both auto-commit and auto-rollback are enabled.");
		}
	}

	public async Task BeginAsync()
	{
		switch (_state)
		{
			case TransactionState.Active:
				throw new InvalidOperationException(
					"Unable to begin transaction. Transaction is active.");
			case TransactionState.Committed:
				throw new InvalidOperationException(
					"Unable to begin transaction. " +
					"Context is in commited state and multiple transactions per context are not allowed.");
			case TransactionState.RolledBack:
				throw new InvalidOperationException(
					"Unable to begin transaction. " +
					"Context is in rolled back state and multiple transactions per context are not allowed.");
			case TransactionState.NotStarted:
				await _connectionBroker.BeginTransactionAsync();

				break;

			default:
				throw new ArgumentOutOfRangeException();
		}

		_state = TransactionState.Active;
	}

	public async Task CommitAsync()
	{
		switch (_state)
		{
			case TransactionState.NotStarted:
				throw new InvalidOperationException(
					"Unable to commit transaction. Transaction has not started.");
			case TransactionState.Committed:
				throw new InvalidOperationException(
					"Unable to commit transaction. Context is in commited state.");
			case TransactionState.RolledBack:
				throw new InvalidOperationException(
					"Unable to commit transaction. Context is in rolled back state.");
			case TransactionState.Active:
				await _connectionBroker.CommitTransactionAsync();

				break;

			default:
				throw new ArgumentOutOfRangeException();
		}

		_state = _options.AllowMultipleTransactions ? TransactionState.NotStarted : TransactionState.Committed;
	}

	public async Task RollbackAsync()
	{
		switch (_state)
		{
			case TransactionState.NotStarted:
				throw new InvalidOperationException(
					"Unable to rollback transaction. Transaction has not started.");
			case TransactionState.Committed:
				throw new InvalidOperationException(
					"Unable to rollback transaction. Context is in commited state.");
			case TransactionState.RolledBack:
				throw new InvalidOperationException(
					"Unable to rollback transaction. Context is in rolled back state.");
			case TransactionState.Active:
				await _connectionBroker.RollbackTransactionAsync();

				break;

			default:
				throw new ArgumentOutOfRangeException();
		}

		_state = _options.AllowMultipleTransactions ? TransactionState.NotStarted : TransactionState.RolledBack;
	}

	public async ValueTask DisposeAsync()
	{
		if (_options.AutoCommitTransaction && _state == TransactionState.Active)
		{
			await _connectionBroker.CommitTransactionAsync();
		}

		if (_options.AutoRollbackTransaction && _state == TransactionState.Active)
		{
			await _connectionBroker.RollbackTransactionAsync();
		}
	}
}
