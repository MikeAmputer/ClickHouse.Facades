using ClickHouse.Facades.Migrations;
using ClickHouse.Facades.Testing;
using Moq;

namespace ClickHouse.Facades.Tests;

[TestClass]
public class ClickHouseMigratorTests : ClickHouseFacadesTestsCore
{
	[TestMethod]
	public async Task NoMigrations_ApplyMigrations_MigrationsTableCreated()
	{
		var databaseName = "test";
		SetupMigrations(databaseName, rollbackOnMigrationFail: false);
		SetupAppliedMigrations(databaseName);


		await GetService<IClickHouseMigrator>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(3, connectionTracker.RecordsCount);

		Assert.AreEqual(
			$"create database if not exists {databaseName}\nengine = Atomic",
			connectionTracker.GetRecord(1).Sql);

		Assert.IsTrue(
			connectionTracker
				.GetRecord(2)
				.Sql
				.StartsWith($"create table if not exists {databaseName}.db_migrations_history"));
	}

	[TestMethod]
	public async Task OneMigrationToApply_ApplyMigrations_MigrationApplied()
	{
		var databaseName = "test";

		Mock<_1_FirstMigration> migrationMock = new();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration"));

		SetupMigrations(databaseName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName);


		await GetService<IClickHouseMigrator>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(5, connectionTracker.RecordsCount);

		Assert.AreEqual("apply migration", connectionTracker.GetRecord(4).Sql);

		Assert.AreEqual(
			$"insert into {databaseName}.db_migrations_history values "
			+ $"({_1_FirstMigration.MigrationIndex}, '{_1_FirstMigration.MigrationName}', 0)",
			connectionTracker.GetRecord(5).Sql);
	}

	[TestMethod]
	public async Task FailingMigrationToApply_RollbackEnabled_ApplyMigrations_MigrationRolledBack()
	{
		var databaseName = "test";

		Mock<_1_FirstMigration> migrationMock = new();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration"));

		migrationMock
			.Setup(m => m.Down(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("rollback migration"));

		SetupMigrations(databaseName, rollbackOnMigrationFail: true, migrationMock.Object);
		SetupAppliedMigrations(databaseName);

		MockExecuteNonQuery<ClickHouseMigrationContext>(
			sql => sql == "apply migration",
			() => throw new Exception("test exception"));


		await Assert.ThrowsExceptionAsync<AggregateException>(
			() => GetService<IClickHouseMigrator>().ApplyMigrationsAsync());


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(5, connectionTracker.RecordsCount);

		Assert.AreEqual("rollback migration", connectionTracker.GetRecord(4).Sql);

		Assert.AreEqual(
			$"insert into {databaseName}.db_migrations_history values "
			+ $"({_1_FirstMigration.MigrationIndex}, '{_1_FirstMigration.MigrationName}', 1)",
			connectionTracker.GetRecord(5).Sql);
	}

	[TestMethod]
	public async Task FailingMigrationToApply_RollbackDisabled_ApplyMigrations_Throws()
	{
		var databaseName = "test";

		Mock<_1_FirstMigration> migrationMock = new();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration"));

		migrationMock
			.Setup(m => m.Down(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("rollback migration"));

		SetupMigrations(databaseName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName);

		MockExecuteNonQuery<ClickHouseMigrationContext>(
			sql => sql == "apply migration",
			() => throw new Exception("test exception"));


		await Assert.ThrowsExceptionAsync<AggregateException>(
			() => GetService<IClickHouseMigrator>().ApplyMigrationsAsync());


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(3, connectionTracker.RecordsCount);
	}

	private void SetupMigrations(
		string databaseName,
		bool rollbackOnMigrationFail,
		params ClickHouseMigration[] migrations)
	{
		Mock<IClickHouseMigrationInstructions> migrationInstructionsMock = new();
		migrationInstructionsMock
			.Setup(m => m.GetConnectionString())
			.Returns("host=localhost;port=8123;database=test;");
		migrationInstructionsMock
			.Setup(m => m.DatabaseName)
			.Returns(databaseName);
		migrationInstructionsMock
			.Setup(m => m.RollbackOnMigrationFail)
			.Returns(rollbackOnMigrationFail);

		Mock<IClickHouseMigrationsLocator> migrationsLocatorMock = new();
		migrationsLocatorMock
			.Setup(m => m.GetMigrations())
			.Returns(migrations);

		UpdateServiceCollection(services =>
			services.AddClickHouseTestMigrations(migrationInstructionsMock.Object, migrationsLocatorMock.Object));
	}

	private void SetupAppliedMigrations(string databaseName, params AppliedMigration[] appliedMigrations)
	{
		MockExecuteReader<ClickHouseMigrationContext, AppliedMigration>(
			sql => sql == $"select id, name from {databaseName}.db_migrations_history final",
			appliedMigrations,
			("id", typeof(ulong), m => m.Id),
			("name", typeof(string), m => m.Name));
	}
}
