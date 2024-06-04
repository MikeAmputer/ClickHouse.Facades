using Microsoft.Extensions.Options;

namespace ClickHouse.Facades.Example;

public class ExampleContextFactory : ClickHouseContextFactory<ExampleContext>
{
	private readonly string _connectionString;

	public ExampleContextFactory(IOptions<ClickHouseConfig> config)
	{
		ArgumentNullException.ThrowIfNull(config);

		_connectionString = config.Value.ConnectionString;
	}

	protected override void SetupContextOptions(ClickHouseContextOptionsBuilder<ExampleContext> optionsBuilder)
	{
		optionsBuilder
			.WithConnectionString(_connectionString)
			.ForceSessions()
			.SetupTransactions(options => options
				.AllowMultipleTransactions());
	}
}
