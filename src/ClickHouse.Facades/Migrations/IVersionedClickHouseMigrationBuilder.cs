namespace ClickHouse.Facades.Migrations;

public interface IVersionedClickHouseMigrationBuilder
{
	public void AddRawSqlStatement(string sql);
}
