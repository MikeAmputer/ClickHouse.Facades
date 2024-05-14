using System.Data.Common;
using ClickHouse.Client.ADO;

namespace ClickHouse.Facades;

internal interface ICommandExecutionStrategy
{
	public static ICommandExecutionStrategy Pick(CommandExecutionStrategy strategy) => strategy switch
	{
		CommandExecutionStrategy.Default => new DefaultCommandExecutionStrategy(),
		CommandExecutionStrategy.Cancelable => new CancelableCommandExecutionStrategy(),
		_ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null)
	};

	Task<int> ExecuteNonQueryAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken);

	Task<object> ExecuteScalarAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken);

	Task<DbDataReader> ExecuteDataReaderAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken);
}
