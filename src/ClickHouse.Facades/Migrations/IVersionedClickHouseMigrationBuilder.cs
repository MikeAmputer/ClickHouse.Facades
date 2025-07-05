namespace ClickHouse.Facades.Migrations;

public interface IVersionedClickHouseMigrationBuilder
{
	void AddRawSqlStatement(string sql);

	void AddSqlFileStatements(string filePath);

	void AddSqlFileStatements(string filePath, ISqlStatementParser parser);
}
