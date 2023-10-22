namespace ClickHouse.Facades.Migrations;

public sealed class ClickHouseMigrationBuilder
{
	private readonly List<string> _statements = new();

	internal List<string> Statements => _statements;

	internal static ClickHouseMigrationBuilder Create => new ClickHouseMigrationBuilder();

	private ClickHouseMigrationBuilder()
	{

	}

	public void AddRawSqlStatement(string sql)
	{
		_statements.Add(sql);
	}
}
