namespace ClickHouse.Facades;

internal enum TransactionState
{
	NotStarted = 0,
	Active,
	Committed,
	RolledBack
}
