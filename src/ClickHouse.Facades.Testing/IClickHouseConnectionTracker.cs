namespace ClickHouse.Facades.Testing;

public interface IClickHouseConnectionTracker
{
	IReadOnlyCollection<ClickHouseTestResponse> GetAllRecords();

	ClickHouseTestResponse GetRecord(int index);

	IEnumerable<ClickHouseTestResponse> GetRecordsBySql(string sqlRegexPattern);

	int RecordsCount { get; }
}
