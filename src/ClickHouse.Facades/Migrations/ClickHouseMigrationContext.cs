namespace ClickHouse.Facades.Migrations;

internal class ClickHouseMigrationContext : ClickHouseContext<ClickHouseMigrationContext>
{
	public IClickHouseMigrationFacade MigrationFacade => GetFacadeAbstraction<IClickHouseMigrationFacade>();
}
