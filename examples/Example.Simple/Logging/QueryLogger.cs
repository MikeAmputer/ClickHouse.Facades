using ClickHouse.Client.ADO;
using Microsoft.Extensions.Logging;

namespace ClickHouse.Facades.Example;

public class QueryLogger : IClickHouseCommandExecutionListener
{
	private readonly ILogger _logger;

	public QueryLogger(ILogger<QueryLogger> logger)
	{
		ArgumentNullException.ThrowIfNull(logger);

		_logger = logger;
	}

	public async Task ProcessExecutedCommand(ClickHouseCommand command, CancellationToken cancellationToken = default)
	{
		await Task.Yield();

		var stats = command.QueryStats;

		if (stats != null && stats.ReadRows > 0)
		{
			_logger.LogDebug($"{command.CommandText}\nReadRows: {stats.ReadRows} | ReadBytes: {stats.ReadBytes}");
		}
	}
}
