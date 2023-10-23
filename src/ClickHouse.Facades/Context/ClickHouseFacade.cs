using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

public abstract class ClickHouseFacade<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private const string UseSessionConnectionStringParameter = "usesession";

	private ClickHouseConnection? _connection = null;
	private bool _sessionEnabled = false;

	internal ClickHouseFacade<TContext> SetConnection(ClickHouseConnection connection)
	{
		ExceptionHelpers.ThrowIfNull(connection);

		if (_connection != null)
		{
			throw new InvalidOperationException($"{GetType()} is already connected.");
		}

		_connection = connection;

		_sessionEnabled = connection.ConnectionString
			.GetConnectionStringParameters()
			.Contains(new KeyValuePair<string, string?>(UseSessionConnectionStringParameter, true.ToString()));

		return this;
	}

	protected ClickHouseCommand CreateCommand()
	{
		ThrowIfNotConnected();

		return _connection!.CreateCommand();
	}

	protected Task<object> ExecuteScalarAsync(string query, CancellationToken cancellationToken = default)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		return _connection.ExecuteScalarAsync(query);
	}

	/// <exception cref="System.InvalidCastException">Unable to cast object to type T.</exception>
	protected async Task<T> ExecuteScalarAsync<T>(string query, CancellationToken cancellationToken = default)
	{
		var result = await ExecuteScalarAsync(query, cancellationToken);

		return (T) result;
	}

	protected Task<int> ExecuteNonQueryAsync(string statement, CancellationToken cancellationToken = default)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		return _connection.ExecuteStatementAsync(statement);
	}

	protected Task<DbDataReader> ExecuteReaderAsync(string query, CancellationToken cancellationToken = default)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		return _connection.ExecuteReaderAsync(query);
	}

	protected async IAsyncEnumerable<T> ExecuteQueryAsync<T>(
		string query,
		Func<DbDataReader, T> rowSelector,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var reader = await ExecuteReaderAsync(query, cancellationToken);

		if (!reader.HasRows)
		{
			yield break;
		}

		while (await reader.ReadAsync(cancellationToken))
		{
			yield return rowSelector(reader);
		}
	}

	protected DataTable ExecuteDataTable(string query, CancellationToken cancellationToken = default)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		return _connection.ExecuteDataTable(query);
	}

	protected Task<long> BulkInsertAsync(
		string destinationTable,
		IEnumerable<object[]> rows,
		IReadOnlyCollection<string>? columns = null,
		int batchSize = 100000,
		int maxDegreeOfParallelism = 4,
		CancellationToken cancellationToken = default)
	{
		return BulkInsertAsync(
			destinationTable,
			bulkInterface => bulkInterface.WriteToServerAsync(rows, columns, cancellationToken),
			batchSize,
			maxDegreeOfParallelism);
	}

	protected Task<long> BulkInsertAsync(
		string destinationTable,
		IDataReader dataReader,
		int batchSize = 100000,
		int maxDegreeOfParallelism = 4,
		CancellationToken cancellationToken = default)
	{
		ExceptionHelpers.ThrowIfNull(dataReader);

		return BulkInsertAsync(
			destinationTable,
			bulkInterface => bulkInterface.WriteToServerAsync(dataReader, cancellationToken),
			batchSize,
			maxDegreeOfParallelism);
	}

	protected Task<long> BulkInsertAsync(
		string destinationTable,
		DataTable dataTable,
		int batchSize = 100000,
		int maxDegreeOfParallelism = 4,
		CancellationToken cancellationToken = default)
	{
		ExceptionHelpers.ThrowIfNull(dataTable);

		return BulkInsertAsync(
			destinationTable,
			bulkInterface => bulkInterface.WriteToServerAsync(dataTable, cancellationToken),
			batchSize,
			maxDegreeOfParallelism);
	}

	private async Task<long> BulkInsertAsync(
		string destinationTable,
		Func<ClickHouseBulkCopy, Task> saveAction,
		int batchSize,
		int maxDegreeOfParallelism)
	{
		ThrowIfNotConnected();

		if (destinationTable.IsNullOrWhiteSpace())
		{
			throw new ArgumentException($"{nameof(destinationTable)} is null or whitespace.");
		}

		if (batchSize < 1)
		{
			throw new ArgumentException($"{nameof(batchSize)} is lower than 1.");
		}

		switch (maxDegreeOfParallelism)
		{
			case < 1:
				throw new ArgumentException($"{nameof(maxDegreeOfParallelism)} is lower than 1.");
			case > 1 when _sessionEnabled:
				throw new InvalidOperationException($"Sessions are not compatible with parallel insertion.");
		}

		using var bulkCopyInterface = new ClickHouseBulkCopy(_connection);
		bulkCopyInterface.DestinationTableName = destinationTable;
		bulkCopyInterface.BatchSize = batchSize;
		bulkCopyInterface.MaxDegreeOfParallelism = maxDegreeOfParallelism;

		await saveAction(bulkCopyInterface);

		return bulkCopyInterface.RowsWritten;
	}

	private void ThrowIfNotConnected()
	{
		if (_connection == null)
		{
			throw new InvalidOperationException($"{GetType()} is not connected.");
		}
	}
}
