using System.Text.RegularExpressions;

namespace ClickHouse.Facades.Testing;

internal class ClickHouseConnectionTracker<TContext> : IClickHouseConnectionTracker
	where TContext : ClickHouseContext<TContext>
{
	private readonly Dictionary<int, ClickHouseTestResponse> _records = new();

	internal void Add(ClickHouseTestResponse record)
	{
		_records.Add(++RecordsCount, record);
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

	public int RecordsCount { get; private set; } = 0;
}
