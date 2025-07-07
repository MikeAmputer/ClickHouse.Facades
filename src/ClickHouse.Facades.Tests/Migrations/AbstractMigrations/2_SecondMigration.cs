using ClickHouse.Facades.Migrations;
using Moq;

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

	public static Mock<_2_SecondMigration> AsMock()
	{
		var mock =  new Mock<_2_SecondMigration>();

		mock.Setup(m => m.Index).Returns(MigrationIndex);
		mock.Setup(m => m.Name).Returns(MigrationName);

		return mock;
	}
}
