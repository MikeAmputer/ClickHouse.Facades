using System.Data.Common;
using ClickHouse.Client;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;

namespace ClickHouse.Facades;

internal class CancelableCommandExecutionStrategy : ICommandExecutionStrategy
{
	public Task<int> ExecuteNonQueryAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken) =>
		Execute(
			connection,
			command,
			(cmd, ct) => cmd.ExecuteNonQueryAsync(ct),
			cancellationToken);

	public Task<object> ExecuteScalarAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken) =>
		Execute(
			connection,
			command,
			(cmd, ct) => cmd.ExecuteScalarAsync(ct),
			cancellationToken);

	public Task<DbDataReader> ExecuteDataReaderAsync(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken) =>
		Execute(
			connection,
			command,
			(cmd, ct) => cmd.ExecuteReaderAsync(ct),
			cancellationToken);

	private static Task<T> Execute<T>(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		Func<ClickHouseCommand, CancellationToken, Task<T>> resultTaskProvider,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		command.QueryId = GenerateQueryId();

		var cancelableTask = resultTaskProvider(command, cancellationToken);

		if (cancellationToken == CancellationToken.None)
		{
			return cancelableTask;
		}

		cancelableTask
			.ContinueWith(_ =>
				{
					KillQuery(connection, command.QueryId);
				},
				CancellationToken.None,
				TaskContinuationOptions.OnlyOnCanceled,
				TaskScheduler.Current)
			.ConfigureAwait(false);

		return cancelableTask;
	}

	private static string GenerateQueryId() => Guid.NewGuid().ToString();

	private static void KillQuery(IClickHouseConnection connection, string queryId)
	{
		var command = connection.CreateCommand();
		command.CommandText = "kill query where query_id = {queryId:String}";
		command.AddParameter("queryId", queryId);
		command.ExecuteNonQuery();
	}
}
