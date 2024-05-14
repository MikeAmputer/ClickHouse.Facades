using System.Data.Common;
using ClickHouse.Client.ADO;

namespace ClickHouse.Facades;

public class DefaultCommandExecutionStrategy : ICommandExecutionStrategy
{
	public Task<int> ExecuteNonQueryAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return command.ExecuteNonQueryAsync(cancellationToken);
	}

	public Task<object> ExecuteScalarAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return command.ExecuteScalarAsync(cancellationToken);
	}

	public Task<DbDataReader> ExecuteDataReaderAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return command.ExecuteReaderAsync(cancellationToken);
	}
}
