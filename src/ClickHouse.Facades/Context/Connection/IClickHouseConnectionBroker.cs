using System.Data;
using System.Data.Common;
using ClickHouse.Driver;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Copy;

namespace ClickHouse.Facades;

internal interface IClickHouseConnectionBroker
{
	ClickHouseCommand CreateCommand();

	Task<object> ExecuteScalarAsync(
		string query,
		Dictionary<string, object?>? parameters,
		CancellationToken cancellationToken);

	Task<int> ExecuteNonQueryAsync(
		string statement,
		Dictionary<string, object?>? parameters,
		CancellationToken cancellationToken);

	Task<DbDataReader> ExecuteReaderAsync(
		string query,
		Dictionary<string, object?>? parameters,
		CancellationToken cancellationToken);

	DataTable ExecuteDataTable(
		string query,
		Dictionary<string, object?>? parameters,
		CancellationToken cancellationToken);

	Task<long> BulkInsertAsync(
		string destinationTable,
		IEnumerable<string> columns,
		IEnumerable<object[]> rows,
		InsertOptions options,
		CancellationToken cancellationToken);

	Task SetSessionParameterAsync(string parameterName, object value);

	Task BeginTransactionAsync();

	Task CommitTransactionAsync();

	Task RollbackTransactionAsync();
}
