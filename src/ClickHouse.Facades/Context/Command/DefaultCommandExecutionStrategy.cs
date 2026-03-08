using System.Data.Common;
using ClickHouse.Driver;
using ClickHouse.Driver.ADO;

namespace ClickHouse.Facades;

internal class DefaultCommandExecutionStrategy : ICommandExecutionStrategy
{
	public Task<int> ExecuteNonQueryAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return command.ExecuteNonQueryAsync(cancellationToken);
	}

	public Task<object> ExecuteScalarAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return command.ExecuteScalarAsync(cancellationToken);
	}

	public Task<DbDataReader> ExecuteDataReaderAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return command.ExecuteReaderAsync(cancellationToken);
	}

	public Task<ClickHouseRawResult> ExecuteRawResultAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return command.ExecuteRawResultAsync(cancellationToken);
	}
}
