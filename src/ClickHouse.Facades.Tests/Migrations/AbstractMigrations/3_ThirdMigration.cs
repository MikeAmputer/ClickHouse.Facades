using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Tests;

[ClickHouseMigration(MigrationIndex, MigrationName)]
internal abstract class _3_ThirdMigration : ClickHouseMigration
{
	public const ulong MigrationIndex = 3;
	public const string MigrationName = "ThirdMigration";

	public static AppliedMigration AsApplied()
	{
		return new AppliedMigration(MigrationIndex, MigrationName);
	}
}
