namespace ClickHouse.Facades;

public sealed class TransactionBrokerOptions
{
	internal TransactionBrokerOptions()
	{

	}

	internal bool AllowMultipleTransactions { get; set; } = false;

	internal bool AutoBeginTransaction { get; set; } = false;

	internal bool AutoRollbackTransaction { get; set; } = false;

	internal bool AutoCommitTransaction { get; set; } = false;
}
