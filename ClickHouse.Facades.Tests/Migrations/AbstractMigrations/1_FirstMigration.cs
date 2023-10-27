using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Tests;

[ClickHouseMigration(MigrationIndex, MigrationName)]
internal abstract class _1_FirstMigration : ClickHouseMigration
{
	public const ulong MigrationIndex = 1;
	public const string MigrationName = "FirstMigration";

	public static AppliedMigration AsApplied()
	{
		return new AppliedMigration(MigrationIndex, MigrationName);
	}
}
