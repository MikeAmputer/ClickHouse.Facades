namespace ClickHouse.Facades;

public sealed class ClickHouseContextOptions<TContext>
	where TContext : ClickHouseContext<TContext>
{
	internal string ConnectionString { get; set; } = "";
	internal bool AllowDatabaseChanges { get; set; } = false;

	internal ClickHouseFacadeRegistry<TContext> FacadeRegistry { get; set; } = new();

	internal HttpClient? HttpClient { get; set; }
	internal IHttpClientFactory? HttpClientFactory { get; set; }
	internal string? HttpClientName { get; set; }
}
