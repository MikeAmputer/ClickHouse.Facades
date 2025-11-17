using System.Data;
using System.Data.Common;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Copy;
using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades.Testing;

internal class ClickHouseConnectionBrokerStub<TContext> : IClickHouseConnectionBroker
	where TContext : ClickHouseContext<TContext>
{
	private readonly ClickHouseConnectionTracker<TContext> _tracker;
	private readonly ClickHouseConnectionResponseProducer<TContext> _responseProducer;

	public ClickHouseConnectionBrokerStub(IServiceProvider serviceProvider)
	{
		_tracker = serviceProvider.GetRequiredService<ClickHouseConnectionTracker<TContext>>();
		_responseProducer = serviceProvider.GetRequiredService<ClickHouseConnectionResponseProducer<TContext>>();
	}

	public string? ServerVersion => _responseProducer.ServerVersion;

	public string? ServerTimezone => _responseProducer.ServerTimezone;

	public ClickHouseCommand CreateCommand()
	{
		throw new NotImplementedException();
	}

	public Task<int> ExecuteNonQueryAsync(
		string statement,
		Dictionary<string, object?>? parameters,
		CancellationToken cancellationToken)
	{
		var result = _responseProducer.TryGetResponse(TestQueryType.ExecuteNonQuery, statement, out var response)
			? (int) response!
			: 0;

		_tracker.Add(new ClickHouseTestResponse(
			TestQueryType.ExecuteNonQuery,
			statement,
			parameters,
			result));

		return Task.FromResult(result);
	}

	public Task<object> ExecuteScalarAsync(
		string query,
		Dictionary<string, object?>? parameters,
		CancellationToken cancellationToken)
	{
		var result = _responseProducer.TryGetResponse(TestQueryType.ExecuteScalar, query, out var response)
			? response!
			: 0;

		_tracker.Add(new ClickHouseTestResponse(
			TestQueryType.ExecuteScalar,
			query,
			parameters,
			result));

		return Task.FromResult(result);
	}

	public Task<DbDataReader> ExecuteReaderAsync(
		string query,
		Dictionary<string, object?>? parameters,
		CancellationToken cancellationToken)
	{
		var result = _responseProducer
			.TryGetResponse(TestQueryType.ExecuteReader, query, out var response)
			? (DataTable) response!
			: new DataTable();

		_tracker.Add(new ClickHouseTestResponse(
			TestQueryType.ExecuteReader,
			query,
			parameters,
			result));

		return Task.FromResult((DbDataReader) new DataTableReader(result));
	}

	public DataTable ExecuteDataTable(
		string query,
		Dictionary<string, object?>? parameters,
		CancellationToken cancellationToken)
	{
		var result = _responseProducer
			.TryGetResponse(TestQueryType.ExecuteReader, query, out var response)
			? (DataTable) response!
			: new DataTable();

		_tracker.Add(new ClickHouseTestResponse(
			TestQueryType.ExecuteReader,
			query,
			parameters,
			result));

		return result;
	}

	public Task<long> BulkInsertAsync(
		string destinationTable,
		Func<ClickHouseBulkCopy, Task> saveAction,
		int batchSize,
		int maxDegreeOfParallelism,
		IReadOnlyCollection<string>? columnNames = null)
	{
		throw new NotImplementedException();
	}

	public Task SetSessionParameterAsync(string parameterName, object value)
	{
		return Task.CompletedTask;
	}

	public Task BeginTransactionAsync()
	{
		return Task.CompletedTask;
	}

	public Task CommitTransactionAsync()
	{
		return Task.CompletedTask;
	}

	public Task RollbackTransactionAsync()
	{
		return Task.CompletedTask;
	}
}
