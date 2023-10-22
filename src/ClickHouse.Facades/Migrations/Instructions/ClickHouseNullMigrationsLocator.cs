namespace ClickHouse.Facades.Migrations;

public sealed class ClickHouseNullMigrationsLocator : IClickHouseMigrationsLocator
{
	public IEnumerable<ClickHouseMigration> GetMigrations() => Enumerable.Empty<ClickHouseMigration>();
}
