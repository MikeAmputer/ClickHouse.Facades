namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrationsLocator
{
	IEnumerable<ClickHouseMigration> GetMigrations();
}
