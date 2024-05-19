using System.Data.Common;
using ClickHouse.Client;
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
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken);

	Task<object> ExecuteScalarAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken);

	Task<DbDataReader> ExecuteDataReaderAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken);
}
