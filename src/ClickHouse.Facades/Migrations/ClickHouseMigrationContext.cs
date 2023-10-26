namespace ClickHouse.Facades.Migrations;

internal class ClickHouseMigrationContext : ClickHouseContext<ClickHouseMigrationContext>
{
	public ClickHouseMigrationFacade MigrationFacade => GetFacade<ClickHouseMigrationFacade>();
}
