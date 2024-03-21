using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using Microsoft.Extensions.DependencyInjection;

namespace ClickHouse.Facades.Testing;

internal class ClickHouseConnectionBrokerStub<TContext> : ClickHouseConnectionBroker
	where TContext : ClickHouseContext<TContext>
{
	private readonly ClickHouseConnectionTracker<TContext> _tracker;
	private readonly ClickHouseConnectionResponseProducer<TContext> _responseProducer;

	public ClickHouseConnectionBrokerStub(
		IServiceProvider serviceProvider,
		ClickHouseConnection connection) : base(connection)
	{
		_tracker = serviceProvider.GetRequiredService<ClickHouseConnectionTracker<TContext>>();
		_responseProducer = serviceProvider.GetRequiredService<ClickHouseConnectionResponseProducer<TContext>>();
	}

	internal override string? ServerVersion => _responseProducer.ServerVersion;

	internal override string? ServerTimezone => _responseProducer.ServerTimezone;

	internal override ClickHouseCommand CreateCommand()
	{
		throw new NotImplementedException();
	}

	internal override Task<int> ExecuteNonQueryAsync(
		string statement,
		Dictionary<string, object>? parameters,
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

	internal override Task<object> ExecuteScalarAsync(
		string query,
		Dictionary<string, object>? parameters,
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

	internal override Task<DbDataReader> ExecuteReaderAsync(
		string query,
		Dictionary<string, object>? parameters,
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

	internal override DataTable ExecuteDataTable(
		string query,
		Dictionary<string, object>? parameters,
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

	internal override Task<long> BulkInsertAsync(
		string destinationTable,
		Func<ClickHouseBulkCopy, Task> saveAction,
		int batchSize,
		int maxDegreeOfParallelism,
		IReadOnlyCollection<string>? columnNames = null)
	{
		throw new NotImplementedException();
	}
}
