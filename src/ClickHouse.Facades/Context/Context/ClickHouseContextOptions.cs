using ClickHouse.Client.ADO;

namespace ClickHouse.Facades;

public sealed class ClickHouseContextOptions<TContext>
	where TContext : ClickHouseContext<TContext>
{
	internal ClickHouseContextOptions()
	{

	}

	internal string ConnectionString { get; set; } = "";
	internal bool AllowDatabaseChanges { get; set; } = false;

	internal HttpClient? HttpClient { get; set; }
	internal IHttpClientFactory? HttpClientFactory { get; set; }
	internal string? HttpClientName { get; set; }

	internal ClickHouseFacadeFactory<TContext> FacadeFactory { get; set; } = null!;

	internal Func<ClickHouseConnection, ICommandExecutionStrategy, IClickHouseConnectionBroker> ConnectionBrokerProvider
	{
		get;
		set;
	} = null!;

	internal CommandExecutionStrategy CommandExecutionStrategy { get; set; } = CommandExecutionStrategy.Default;

	internal TransactionBrokerOptions TransactionBrokerOptions { get; set; } = null!;

	internal bool ParametersInBody { get; set; } = false;
}
