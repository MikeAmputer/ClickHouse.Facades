using ClickHouse.Facades.Migrations;
using Moq;

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

	public static Mock<_1_FirstMigration> AsMock()
	{
		var mock =  new Mock<_1_FirstMigration>();

		mock.Setup(m => m.Index).Returns(MigrationIndex);
		mock.Setup(m => m.Name).Returns(MigrationName);

		return mock;
	}
}
