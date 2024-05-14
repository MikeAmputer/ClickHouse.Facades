using System.Data.Common;
using ClickHouse.Client.ADO;

namespace ClickHouse.Facades.Testing;

internal class CommandExecutionStrategyStub : ICommandExecutionStrategy
{
	public Task<int> ExecuteNonQueryAsync(ClickHouseConnection connection, ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<object> ExecuteScalarAsync(ClickHouseConnection connection, ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<DbDataReader> ExecuteDataReaderAsync(ClickHouseConnection connection, ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
