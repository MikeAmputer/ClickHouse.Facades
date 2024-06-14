using Microsoft.Extensions.Options;

namespace ClickHouse.Facades.Example;

public class ExampleContextFactory : ClickHouseContextFactory<ExampleContext>
{
	private readonly string _connectionStringMain;
	private readonly string _connectionStringShard;

	public bool UseShardServer { get; set; } = false;

	public ExampleContextFactory(IOptions<ClickHouseMainConfig> mainConfig, IOptions<ClickHouseShardConfig> shardConfig)
	{
		ArgumentNullException.ThrowIfNull(mainConfig);
		ArgumentNullException.ThrowIfNull(shardConfig);

		_connectionStringMain = mainConfig.Value.ConnectionString;
		_connectionStringShard = shardConfig.Value.ConnectionString;
	}

	protected override void SetupContextOptions(ClickHouseContextOptionsBuilder<ExampleContext> optionsBuilder)
	{
		optionsBuilder
			.WithConnectionString(UseShardServer ? _connectionStringShard : _connectionStringMain)
			.ForceSessions()
			.SetupTransactions(options => options
				.AllowMultipleTransactions()
				.AutoRollback());
	}
}
