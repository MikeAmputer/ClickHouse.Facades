namespace ClickHouse.Facades.Example;

public class DistributedFacade : ClickHouseFacade<ExampleContext>
{
	public Task InsertValues(int count, CancellationToken cancellationToken = default)
	{
		return ExecuteNonQueryAsync(
			"insert into example_dist_table select number as value from numbers({count:Int32})",
			new { count },
			cancellationToken);
	}

	public ValueTask<int[]> GetValues(CancellationToken cancellationToken = default)
	{
		return ExecuteQueryAsync(
				"select * from example_dist_table",
				reader => reader.GetInt32(0),
				cancellationToken)
			.ToArrayAsync(cancellationToken);
	}
}
