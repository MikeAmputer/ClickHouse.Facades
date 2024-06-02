using ClickHouse.Client.ADO;

namespace ClickHouse.Facades;

public sealed class ClickHouseContextOptions<TContext>
	where TContext : ClickHouseContext<TContext>
{
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

	internal CommandExecutionStrategy CommandExecutionStrategy = CommandExecutionStrategy.Default;
}
