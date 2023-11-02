namespace ClickHouse.Facades.Testing;

public interface IClickHouseConnectionTracker
{
	IReadOnlyCollection<ClickHouseTestResponse> GetAllRecords();

	ClickHouseTestResponse GetRecord(int index);

	int RecordsCount { get; }
}
