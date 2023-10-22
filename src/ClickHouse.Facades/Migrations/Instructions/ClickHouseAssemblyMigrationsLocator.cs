using System.Reflection;

namespace ClickHouse.Facades.Migrations;

public abstract class ClickHouseAssemblyMigrationsLocator : IClickHouseMigrationsLocator
{
	protected abstract Assembly TargetAssembly { get; }

	public IEnumerable<ClickHouseMigration> GetMigrations()
	{
		return TargetAssembly
			.GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract)
			.Where(t => t.IsSubclassOf(typeof(ClickHouseMigration)))
			.Select(Activator.CreateInstance)
			.Cast<ClickHouseMigration>();
	}
}
