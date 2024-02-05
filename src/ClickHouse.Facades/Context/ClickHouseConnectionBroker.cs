using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

internal class ClickHouseConnectionBroker
{
	private const string UseSessionConnectionStringParameter = "usesession";

	private readonly ClickHouseConnection _connection;
	private readonly bool _sessionEnabled;

	public ClickHouseConnectionBroker(ClickHouseConnection connection)
	{
		if (_connection != null)
		{
			throw new InvalidOperationException($"{GetType()} is already connected.");
		}

		_connection = connection ?? throw new ArgumentNullException(nameof(connection));

		_sessionEnabled = connection.ConnectionString
			.GetConnectionStringParameters()
			.Contains(new KeyValuePair<string, string?>(UseSessionConnectionStringParameter, true.ToString()));
	}

	internal virtual string? ServerVersion => _connection.ServerVersion;

	internal virtual string? ServerTimezone => _connection.ServerTimezone;

	internal virtual ClickHouseCommand CreateCommand()
	{
		ThrowIfNotConnected();

		return _connection.CreateCommand();
	}

	internal virtual Task<object> ExecuteScalarAsync(string query, CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		return _connection.ExecuteScalarAsync(query);
	}

	internal virtual Task<int> ExecuteNonQueryAsync(string statement, CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		return _connection.ExecuteStatementAsync(statement);
	}

	internal virtual Task<DbDataReader> ExecuteReaderAsync(string query, CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		return _connection.ExecuteReaderAsync(query);
	}

	internal virtual DataTable ExecuteDataTable(string query, CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		return _connection.ExecuteDataTable(query);
	}

	internal virtual async Task<long> BulkInsertAsync(
		string destinationTable,
		Func<ClickHouseBulkCopy, Task> saveAction,
		int batchSize,
		int maxDegreeOfParallelism,
		IReadOnlyCollection<string>? columnNames = null)
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

		using var bulkCopyInterface = new ClickHouseBulkCopy(_connection)
		{
			DestinationTableName = destinationTable,
			BatchSize = batchSize,
			MaxDegreeOfParallelism = maxDegreeOfParallelism,
			ColumnNames = columnNames,
		};

		await bulkCopyInterface.InitAsync();
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
