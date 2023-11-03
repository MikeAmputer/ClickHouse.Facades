namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrationsLocator
{
	IEnumerable<ClickHouseMigration> GetMigrations();
}

public interface IClickHouseMigrationsLocator<TContext> : IClickHouseMigrationsLocator
	where TContext : ClickHouseContext<TContext>
{

}
