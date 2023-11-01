namespace ClickHouse.Facades.Testing;

public enum TestQueryType
{
	ExecuteNonQuery = 1,
	ExecuteScalar,
	ExecuteReader,
}

public class ClickHouseTestResponse
{
	public TestQueryType QueryType { get; }
	public string Sql { get; }
	public object? Result { get; }

	internal ClickHouseTestResponse(TestQueryType queryType, string sql, object? result)
	{
		QueryType = queryType;
		Sql = sql;
		Result = result;
	}
}
