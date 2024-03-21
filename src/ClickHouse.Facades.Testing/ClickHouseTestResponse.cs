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
	public Dictionary<string, object>? Parameters { get; set; }
	public object? Result { get; }

	internal ClickHouseTestResponse(
		TestQueryType queryType,
		string sql,
		Dictionary<string, object>? parameters,
		object? result)
	{
		QueryType = queryType;
		Sql = sql;
		Parameters = parameters;
		Result = result;
	}
}
