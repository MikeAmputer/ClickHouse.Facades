using ClickHouse.Driver.ADO;

namespace ClickHouse.Facades;

public sealed class ClickHouseContextOptions<TContext>
	where TContext : ClickHouseContext<TContext>
{
	internal ClickHouseContextOptions()
	{

	}

	internal ClickHouseClientSettings ClickHouseClientSettings { get; init; } = null!;

	internal bool AllowDatabaseChanges { get; init; } = false;

	internal ClickHouseFacadeFactory<TContext> FacadeFactory { get; init; } = null!;

	internal Func<ConnectionBrokerParameters, IClickHouseConnectionBroker> ConnectionBrokerProvider
	{
		get;
		init;
	} = null!;

	internal CommandExecutionStrategy CommandExecutionStrategy { get; init; } = CommandExecutionStrategy.Default;

	internal IClickHouseCommandExecutionListener? CommandExecutionListener { get; init; } = null;

	internal TransactionBrokerOptions TransactionBrokerOptions { get; init; } = null!;
}
