using ClickHouse.Facades.Migrations;
using Moq;

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

	public static Mock<_3_ThirdMigration> AsMock()
	{
		var mock =  new Mock<_3_ThirdMigration>();

		mock.Setup(m => m.Index).Returns(MigrationIndex);
		mock.Setup(m => m.Name).Returns(MigrationName);

		return mock;
	}
}
