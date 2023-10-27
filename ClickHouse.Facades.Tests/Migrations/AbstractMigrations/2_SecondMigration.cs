using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Tests;

[ClickHouseMigration(MigrationIndex, MigrationName)]
internal abstract class _2_SecondMigration : ClickHouseMigration
{
	public const ulong MigrationIndex = 2;
	public const string MigrationName = "SecondMigration";

	public static AppliedMigration AsApplied()
	{
		return new AppliedMigration(MigrationIndex, MigrationName);
	}
}
