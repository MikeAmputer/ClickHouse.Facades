namespace ClickHouse.Facades.Example;

public class TargetFacade : ClickHouseFacade<ExampleContext>
{
	public Task InsertValue(int value, CancellationToken cancellationToken = default)
	{
		return ExecuteNonQueryAsync(
			"insert into example_table format Values ({value:Int32})",
			new { value },
			cancellationToken);
	}

	public ValueTask<int[]> GetValues(CancellationToken cancellationToken = default)
	{
		return ExecuteQueryAsync(
				"select * from example_table",
				reader => reader.GetInt32(0),
				cancellationToken)
			.ToArrayAsync(cancellationToken);
	}

	public Task Truncate(CancellationToken cancellationToken = default)
	{
		return ExecuteNonQueryAsync("truncate table example_table", cancellationToken);
	}
}
