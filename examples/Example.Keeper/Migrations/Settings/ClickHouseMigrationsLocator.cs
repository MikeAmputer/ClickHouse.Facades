using System.Reflection;
using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Example;

public class ClickHouseMigrationsLocator : ClickHouseAssemblyMigrationsLocator
{
	protected override Assembly TargetAssembly => typeof(ClickHouseMigrationsLocator).Assembly;
}
