namespace ClickHouse.Facades;

public sealed class TransactionBrokerOptions
{
	internal TransactionBrokerOptions()
	{

	}

	internal bool AllowMultipleTransactions { get; init; } = false;

	internal bool AutoBeginTransaction { get; init; } = false;

	internal bool AutoRollbackTransaction { get; init; } = false;

	internal bool AutoCommitTransaction { get; init; } = false;
}
