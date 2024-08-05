using Microsoft.Extensions.Options;

namespace ClickHouse.Facades.Example;

public class ExampleContextFactory : ClickHouseContextFactory<ExampleContext>
{
	private readonly string _connectionString;
	private readonly IClickHouseCommandExecutionListener _commandExecutionListener;

	public ExampleContextFactory(IOptions<ClickHouseConfig> config, QueryLogger queryLogger)
	{
		ArgumentNullException.ThrowIfNull(config);
		ArgumentNullException.ThrowIfNull(queryLogger);

		_connectionString = config.Value.ConnectionString;
		_commandExecutionListener = queryLogger;
	}

	protected override void SetupContextOptions(ClickHouseContextOptionsBuilder<ExampleContext> optionsBuilder)
	{
		optionsBuilder
			.WithConnectionString(_connectionString)
			.WithCommandExecutionStrategy(CommandExecutionStrategy.Cancelable)
			.WithCommandExecutionListener(_commandExecutionListener);
	}
}
