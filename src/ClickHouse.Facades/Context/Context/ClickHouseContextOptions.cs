namespace ClickHouse.Facades;

public sealed class ClickHouseContextOptions<TContext>
	where TContext : ClickHouseContext<TContext>
{
	internal ClickHouseContextOptions()
	{

	}

	internal string ConnectionString { get; init; } = "";
	internal bool AllowDatabaseChanges { get; init; } = false;

	internal HttpClient? HttpClient { get; init; }
	internal IHttpClientFactory? HttpClientFactory { get; init; }
	internal string? HttpClientName { get; init; }

	internal ClickHouseFacadeFactory<TContext> FacadeFactory { get; init; } = null!;

	internal Func<ConnectionBrokerParameters, IClickHouseConnectionBroker> ConnectionBrokerProvider
	{
		get;
		init;
	} = null!;

	internal CommandExecutionStrategy CommandExecutionStrategy { get; init; } = CommandExecutionStrategy.Default;

	internal IClickHouseCommandExecutionListener? CommandExecutionListener { get; init; } = null;

	internal TransactionBrokerOptions TransactionBrokerOptions { get; init; } = null!;

	internal IDictionary<string, object>? ConnectionCustomSettings = null;

	internal bool ParametersInBody { get; init; } = false;
}
