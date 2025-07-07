namespace ClickHouse.Facades.Migrations;

public abstract class AggregateClickHouseMigrationsLocator : IClickHouseMigrationsLocator
{
	protected abstract IEnumerable<IClickHouseMigrationsLocator> Locators { get; }

	public IEnumerable<ClickHouseMigration> GetMigrations()
	{
		return Locators.SelectMany(locator => locator.GetMigrations());
	}
}
