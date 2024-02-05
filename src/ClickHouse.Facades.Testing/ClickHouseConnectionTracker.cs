using System.Text.RegularExpressions;

namespace ClickHouse.Facades.Testing;

internal class ClickHouseConnectionTracker<TContext> : IClickHouseConnectionTracker
	where TContext : ClickHouseContext<TContext>
{
	private readonly Dictionary<int, ClickHouseTestResponse> _records = new();
	private int _recordsCount = 0;

	internal void Add(ClickHouseTestResponse record)
	{
		_records.Add(++_recordsCount, record);
	}

	public IReadOnlyCollection<ClickHouseTestResponse> GetAllRecords()
	{
		return _records.Select(r => r.Value).ToList();
	}

	public ClickHouseTestResponse GetRecord(int index)
	{
		return _records[index];
	}

	public IEnumerable<ClickHouseTestResponse> GetRecordsBySql(string sqlRegexPattern)
	{
		var regex = new Regex(sqlRegexPattern);

		return _records
			.Select(r => r.Value)
			.Where(r => regex.IsMatch(r.Sql));
	}

	public int RecordsCount => _recordsCount;
}
