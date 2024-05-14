using System.Data.Common;
using ClickHouse.Client;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;

namespace ClickHouse.Facades;

public class CancellableCommandExecutionStrategy : ICommandExecutionStrategy
{
	public Task<int> ExecuteNonQueryAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		command.QueryId = GenerateQueryId();

		var result = command.ExecuteNonQueryAsync(cancellationToken);

		return Execute(connection, command, result, cancellationToken);
	}

	public Task<object> ExecuteScalarAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		command.QueryId = GenerateQueryId();

		var result = command.ExecuteScalarAsync(cancellationToken);

		return Execute(connection, command, result, cancellationToken);
	}

	public Task<DbDataReader> ExecuteDataReaderAsync(
		ClickHouseConnection connection,
		ClickHouseCommand command,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		command.QueryId = GenerateQueryId();

		var result = command.ExecuteReaderAsync(cancellationToken);

		return Execute(connection, command, result, cancellationToken);
	}

	private static Task<T> Execute<T>(
		IClickHouseConnection connection,
		ClickHouseCommand command,
		Task<T> result,
		CancellationToken cancellationToken)
	{
		if (cancellationToken == CancellationToken.None)
		{
			return result;
		}

		result
			.ContinueWith(_ =>
				{
					Console.WriteLine(command.QueryId);
					KillQuery(connection, command.QueryId);
				},
				CancellationToken.None,
				TaskContinuationOptions.OnlyOnCanceled,
				TaskScheduler.Current)
			.ConfigureAwait(false);

		return result;
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
