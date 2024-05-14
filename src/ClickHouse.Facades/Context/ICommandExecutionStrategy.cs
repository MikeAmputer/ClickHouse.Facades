using System.Data.Common;
using ClickHouse.Client.ADO;

namespace ClickHouse.Facades;

internal interface ICommandExecutionStrategy
{
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
