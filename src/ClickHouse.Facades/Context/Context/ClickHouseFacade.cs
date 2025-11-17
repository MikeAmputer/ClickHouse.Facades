using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Copy;
using ClickHouse.Facades.Utility;
// ReSharper disable MemberCanBePrivate.Global

namespace ClickHouse.Facades;

public abstract class ClickHouseFacade<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private IClickHouseConnectionBroker _connectionBroker = null!;

	internal void SetConnectionBroker(IClickHouseConnectionBroker connectionBroker)
	{
		if (_connectionBroker != null)
		{
			throw new InvalidOperationException("Connection broker is already set.");
		}

		_connectionBroker = connectionBroker ?? throw new ArgumentNullException(nameof(connectionBroker));
	}

	protected ClickHouseCommand CreateCommand()
	{
		return _connectionBroker.CreateCommand();
	}

	protected Task<object> ExecuteScalarAsync(string query, CancellationToken cancellationToken = default)
	{
		return _connectionBroker.ExecuteScalarAsync(query, null, cancellationToken);
	}

	protected Task<object> ExecuteScalarAsync(
		string query,
		object parameters,
		CancellationToken cancellationToken = default)
	{
		return _connectionBroker.ExecuteScalarAsync(query, parameters.DeconstructToDictionary(), cancellationToken);
	}

	/// <exception cref="System.InvalidCastException">Unable to cast object to type T.</exception>
	protected async Task<T> ExecuteScalarAsync<T>(string query, CancellationToken cancellationToken = default)
	{
		var result = await ExecuteScalarAsync(query, cancellationToken);

		return (T) result;
	}

	/// <exception cref="System.InvalidCastException">Unable to cast object to type T.</exception>
	protected async Task<T> ExecuteScalarAsync<T>(
		string query,
		object parameters,
		CancellationToken cancellationToken = default)
	{
		var result = await ExecuteScalarAsync(query, parameters, cancellationToken);

		return (T) result;
	}

	protected Task<int> ExecuteNonQueryAsync(string statement, CancellationToken cancellationToken = default)
	{
		return _connectionBroker.ExecuteNonQueryAsync(statement, null, cancellationToken);
	}

	protected Task<int> ExecuteNonQueryAsync(
		string statement,
		object parameters,
		CancellationToken cancellationToken = default)
	{
		return _connectionBroker.ExecuteNonQueryAsync(
			statement,
			parameters.DeconstructToDictionary(),
			cancellationToken);
	}

	protected Task<DbDataReader> ExecuteReaderAsync(string query, CancellationToken cancellationToken = default)
	{
		return _connectionBroker.ExecuteReaderAsync(query, null, cancellationToken);
	}

	protected Task<DbDataReader> ExecuteReaderAsync(
		string query,
		object parameters,
		CancellationToken cancellationToken = default)
	{
		return _connectionBroker.ExecuteReaderAsync(query, parameters.DeconstructToDictionary(), cancellationToken);
	}

	protected async IAsyncEnumerable<T> ExecuteQueryAsync<T>(
		string query,
		Func<DbDataReader, T> rowSelector,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await using var reader = await ExecuteReaderAsync(query, cancellationToken);

		if (!reader.HasRows)
		{
			yield break;
		}

		while (await reader.ReadAsync(cancellationToken))
		{
			yield return rowSelector(reader);
		}
	}

	protected async IAsyncEnumerable<T> ExecuteQueryAsync<T>(
		string query,
		object parameters,
		Func<DbDataReader, T> rowSelector,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await using var reader = await ExecuteReaderAsync(query, parameters, cancellationToken);

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
		return _connectionBroker.ExecuteDataTable(query, null, cancellationToken);
	}

	protected DataTable ExecuteDataTable(
		string query,
		object parameters,
		CancellationToken cancellationToken = default)
	{
		return _connectionBroker.ExecuteDataTable(query, parameters.DeconstructToDictionary(), cancellationToken);
	}

	protected Task<long> BulkInsertAsync(
		string destinationTable,
		IEnumerable<object[]> rows,
		IReadOnlyCollection<string>? columnNames = null,
		int batchSize = 100000,
		int maxDegreeOfParallelism = 4,
		CancellationToken cancellationToken = default)
	{
		return BulkInsertAsync(
			destinationTable,
			bulkInterface => bulkInterface.WriteToServerAsync(rows, cancellationToken),
			batchSize,
			maxDegreeOfParallelism,
			columnNames);
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

	private Task<long> BulkInsertAsync(
		string destinationTable,
		Func<ClickHouseBulkCopy, Task> saveAction,
		int batchSize,
		int maxDegreeOfParallelism,
		IReadOnlyCollection<string>? columnNames = null)
	{
		return _connectionBroker.BulkInsertAsync(
			destinationTable,
			saveAction,
			batchSize,
			maxDegreeOfParallelism,
			columnNames);
	}
}
