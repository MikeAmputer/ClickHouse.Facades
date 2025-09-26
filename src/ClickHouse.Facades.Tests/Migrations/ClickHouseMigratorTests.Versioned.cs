using ClickHouse.Facades.Migrations;
using Moq;

namespace ClickHouse.Facades.Tests;

public partial class ClickHouseMigratorTests
{
		[TestMethod]
	public async Task VersionedMigrationToApply_DatabaseVersionMeetsCriteria_MigrationApplied()
	{
		const string databaseName = "test";
		const string historyTableName = "db_migrations_history";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(builder =>
				builder.WhenVersion(
					v => v < "25.1",
					b => b.AddRawSqlStatement("apply migration")));

		SetupMigrations(databaseName, historyTableName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName, historyTableName);

		SetupDatabaseVersion("24.6");


		await GetService<IClickHouseMigrator>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(6, connectionTracker.RecordsCount);
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql("apply migration").Count());
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql($@"insert into \w*\.{historyTableName}").Count());
	}

	[TestMethod]
	public async Task VersionedMigrationToApply_DatabaseVersionMissesCriteria_MigrationNotApplied()
	{
		const string databaseName = "test";
		const string historyTableName = "db_migrations_history";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(builder =>
				builder.WhenVersion(
					v => v > "25.1",
					b => b.AddRawSqlStatement("apply migration")));

		SetupMigrations(databaseName, historyTableName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName, historyTableName);

		SetupDatabaseVersion("24.6");


		await GetService<IClickHouseMigrator>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(5, connectionTracker.RecordsCount);
		Assert.AreEqual(0, connectionTracker.GetRecordsBySql("apply migration").Count());
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql($@"insert into \w*\.{historyTableName}").Count());
	}

	[TestMethod]
	public async Task RangeVersionedMigrationToApply_DatabaseVersionIsInRange_MigrationApplied()
	{
		const string databaseName = "test";
		const string historyTableName = "db_migrations_history";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(builder =>
				builder.ForVersionRange(
					"24.1",
					"25.1",
					b => b.AddRawSqlStatement("apply migration")));

		SetupMigrations(databaseName, historyTableName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName, historyTableName);

		SetupDatabaseVersion("24.6");


		await GetService<IClickHouseMigrator>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(6, connectionTracker.RecordsCount);
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql("apply migration").Count());
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql($@"insert into \w*\.{historyTableName}").Count());
	}

	[TestMethod]
	public async Task RangeVersionedMigrationToApply_DatabaseVersionIsNotInRange_MigrationNotApplied()
	{
		const string databaseName = "test";
		const string historyTableName = "db_migrations_history";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(builder =>
				builder.ForVersionRange(
					"24.1",
					"25.1",
					b => b.AddRawSqlStatement("apply migration")));

		SetupMigrations(databaseName, historyTableName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName, historyTableName);

		SetupDatabaseVersion("25.6");


		await GetService<IClickHouseMigrator>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(5, connectionTracker.RecordsCount);
		Assert.AreEqual(0, connectionTracker.GetRecordsBySql("apply migration").Count());
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql($@"insert into \w*\.{historyTableName}").Count());
	}

	[TestMethod]
	public async Task SinceVersionedMigrationToApply_DatabaseVersionMeetsCriteria_MigrationApplied()
	{
		const string databaseName = "test";
		const string historyTableName = "db_migrations_history";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(builder =>
				builder.SinceVersion(
					"24.6",
					b => b.AddRawSqlStatement("apply migration")));

		SetupMigrations(databaseName, historyTableName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName, historyTableName);

		SetupDatabaseVersion("24.6");


		await GetService<IClickHouseMigrator>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(6, connectionTracker.RecordsCount);
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql("apply migration").Count());
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql($@"insert into \w*\.{historyTableName}").Count());
	}

	[TestMethod]
	public async Task SinceVersionedMigrationToApply_DatabaseVersionIsLower_MigrationNotApplied()
	{
		const string databaseName = "test";
		const string historyTableName = "db_migrations_history";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(builder =>
				builder.SinceVersion(
					"24.6",
					b => b.AddRawSqlStatement("apply migration")));

		SetupMigrations(databaseName, historyTableName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName, historyTableName);

		SetupDatabaseVersion("24.2");


		await GetService<IClickHouseMigrator>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(5, connectionTracker.RecordsCount);
		Assert.AreEqual(0, connectionTracker.GetRecordsBySql("apply migration").Count());
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql($@"insert into \w*\.{historyTableName}").Count());
	}
}
