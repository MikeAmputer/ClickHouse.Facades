namespace ClickHouse.Facades.Migrations;

public interface ISqlStatementParser
{
	IEnumerable<string> ParseStatements(string sqlText);
}
