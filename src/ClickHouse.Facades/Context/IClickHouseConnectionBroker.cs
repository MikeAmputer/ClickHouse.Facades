using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;

namespace ClickHouse.Facades;

internal interface IClickHouseConnectionBroker
{
	string? ServerVersion { get; }

	string? ServerTimezone { get; }

	ClickHouseCommand CreateCommand();

	Task<object> ExecuteScalarAsync(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken);

	Task<int> ExecuteNonQueryAsync(
		string statement,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken);

	Task<DbDataReader> ExecuteReaderAsync(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken);

	DataTable ExecuteDataTable(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken);

	Task<long> BulkInsertAsync(
		string destinationTable,
		Func<ClickHouseBulkCopy, Task> saveAction,
		int batchSize,
		int maxDegreeOfParallelism,
		IReadOnlyCollection<string>? columnNames = null);

	Task SetSessionParameter(string parameterName, object value);
}
