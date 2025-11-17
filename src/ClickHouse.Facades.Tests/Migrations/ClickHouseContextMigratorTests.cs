using ClickHouse.Facades.Migrations;
using ClickHouse.Facades.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ClickHouse.Facades.Tests;

[TestClass]
public class ClickHouseContextMigratorTests : ClickHouseFacadesTestsCore
{
	private const string MigrationsDatabaseName = "test";
	private const string MigrationsHistoryTableName = "db_migrations_history";

	protected override void SetupServiceCollection(IServiceCollection services)
	{
		Mock<IClickHouseMigrationInstructions> migrationInstructionsMock = new();
		migrationInstructionsMock
			.Setup(m => m.GetConnectionString())
			.Returns("host=localhost;port=8123;database=test;");
		migrationInstructionsMock
			.Setup(m => m.DatabaseName)
			.Returns(MigrationsDatabaseName);
		migrationInstructionsMock
			.Setup(m => m.RollbackOnMigrationFail)
			.Returns(false);
		migrationInstructionsMock
			.Setup(m => m.HistoryTableName)
			.Returns(MigrationsHistoryTableName);

		services.AddClickHouseTestMigrations(migrationInstructionsMock.Object);
	}

	[TestMethod]
	public async Task NoMigrations_ApplySingleContextMigrations_MigrationsTableCreated()
	{
		SetupMigrations<TestContext_1>(Enumerable.Empty<ClickHouseMigration>().ToArray());
		SetupAppliedMigrations([]);

		SetupDatabaseVersion();


		await GetService<IClickHouseMigrator<TestContext_1>>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(3, connectionTracker.RecordsCount);

		Assert.AreEqual(
			$"create database if not exists {MigrationsDatabaseName}\nengine = Atomic",
			connectionTracker.GetRecord(1).Sql);

		Assert.StartsWith(
			$"create table if not exists {MigrationsDatabaseName}.{MigrationsHistoryTableName}",
			connectionTracker
				.GetRecord(2)
				.Sql
		);
	}

	[TestMethod]
	public async Task OneMigrationToApply_ApplySingleContextMigrations_MigrationApplied()
	{
		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration"));
		SetupMigrations<TestContext_1>(migrationMock.Object);
		SetupAppliedMigrations([]);

		SetupDatabaseVersion();


		await GetService<IClickHouseMigrator<TestContext_1>>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(6, connectionTracker.RecordsCount);

		Assert.AreEqual("apply migration", connectionTracker.GetRecord(5).Sql);

		Assert.AreEqual(
			$"insert into {MigrationsDatabaseName}.{MigrationsHistoryTableName} values "
			+ $"({_1_FirstMigration.MigrationIndex}, '{_1_FirstMigration.MigrationName}', 0)",
			connectionTracker.GetRecord(6).Sql);
	}

	[TestMethod]
	public async Task TwoContexts_OneMigrationToApplyForEach_ApplySingleContextMigrations_MigrationApplied()
	{
		var firstMigrationMock = _1_FirstMigration.AsMock();
		firstMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 1"));

		var secondMigrationMock = _2_SecondMigration.AsMock();
		secondMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 2"));

		SetupMigrations<TestContext_1>(firstMigrationMock.Object);
		SetupMigrations<TestContext_2>(secondMigrationMock.Object);
		SetupAppliedMigrations([]);

		SetupDatabaseVersion();


		await GetService<IClickHouseMigrator<TestContext_1>>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(6, connectionTracker.RecordsCount);

		Assert.AreEqual("apply migration 1", connectionTracker.GetRecord(5).Sql);

		Assert.AreEqual(
			$"insert into {MigrationsDatabaseName}.{MigrationsHistoryTableName} values "
			+ $"({_1_FirstMigration.MigrationIndex}, '{_1_FirstMigration.MigrationName}', 0)",
			connectionTracker.GetRecord(6).Sql);
	}

	[TestMethod]
	public async Task TwoContexts_OneMigrationToApplyForEach_ApplyBothContextMigrations_MigrationsApplied()
	{
		var firstMigrationMock = _1_FirstMigration.AsMock();
		firstMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 1"));

		var secondMigrationMock = _2_SecondMigration.AsMock();
		secondMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 2"));

		SetupMigrations<TestContext_1>(firstMigrationMock.Object);
		SetupMigrations<TestContext_2>(secondMigrationMock.Object);
		SetupAppliedMigrations([]);

		SetupDatabaseVersion();


		await GetService<IClickHouseMigrator<TestContext_1>>().ApplyMigrationsAsync();
		await GetService<IClickHouseMigrator<TestContext_2>>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(12, connectionTracker.RecordsCount);

		Assert.AreEqual("apply migration 1", connectionTracker.GetRecord(5).Sql);

		Assert.AreEqual(
			$"insert into {MigrationsDatabaseName}.{MigrationsHistoryTableName} values "
			+ $"({_1_FirstMigration.MigrationIndex}, '{_1_FirstMigration.MigrationName}', 0)",
			connectionTracker.GetRecord(6).Sql);

		Assert.AreEqual("apply migration 2", connectionTracker.GetRecord(11).Sql);

		Assert.AreEqual(
			$"insert into {MigrationsDatabaseName}.{MigrationsHistoryTableName} values "
			+ $"({_2_SecondMigration.MigrationIndex}, '{_2_SecondMigration.MigrationName}', 0)",
			connectionTracker.GetRecord(12).Sql);
	}

	[TestMethod]
	public async Task TwoContexts_OneIsUpToDate_ApplyBothContextMigrations_SecondContextMigrationsApplied()
	{
		var firstMigrationMock = _1_FirstMigration.AsMock();
		firstMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 1"));

		var secondMigrationMock = _2_SecondMigration.AsMock();
		secondMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 2"));

		SetupMigrations<TestContext_1>(firstMigrationMock.Object);
		SetupMigrations<TestContext_2>(secondMigrationMock.Object);
		SetupAppliedMigrations([_1_FirstMigration.AsApplied()]);

		SetupDatabaseVersion();


		await GetService<IClickHouseMigrator<TestContext_1>>().ApplyMigrationsAsync();
		await GetService<IClickHouseMigrator<TestContext_2>>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(0, connectionTracker.GetRecordsBySql("apply migration 1").Count());
		Assert.AreEqual(1, connectionTracker.GetRecordsBySql("apply migration 2").Count());
	}

	[TestMethod]
	public async Task TwoContexts_SameMigrationToApply_ApplyBothContextMigrations_MigrationAppliedOnce()
	{
		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 1"));

		SetupMigrations<TestContext_1>(migrationMock.Object);
		SetupMigrations<TestContext_2>(migrationMock.Object);
		SetupAppliedMigrations([]);
		MockExecuteNonQuery<ClickHouseMigrationContext>(
			sql => sql == $"insert into {MigrationsDatabaseName}.{MigrationsHistoryTableName} values "
				+ $"({_1_FirstMigration.MigrationIndex}, '{_1_FirstMigration.MigrationName}', 0)",
			() =>
			{
				SetupAppliedMigrations([_1_FirstMigration.AsApplied()]);

				return 1;
			});

		SetupDatabaseVersion();


		await GetService<IClickHouseMigrator<TestContext_1>>().ApplyMigrationsAsync();
		await GetService<IClickHouseMigrator<TestContext_2>>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(1, connectionTracker.GetRecordsBySql("apply migration 1").Count());
	}

	private void SetupMigrations<TContext>(params ClickHouseMigration[] migrations)
		where TContext : ClickHouseContext<TContext>
	{
		Mock<IClickHouseMigrationsLocator<TContext>> migrationsLocatorMock = new();
		migrationsLocatorMock
			.Setup(m => m.GetMigrations())
			.Returns(migrations);

		UpdateServiceCollection(services =>
			services.AddClickHouseTestContextMigrations<TContext, IClickHouseMigrationsLocator<TContext>>(
				migrationsLocatorMock.Object));
	}

	private void SetupAppliedMigrations(params AppliedMigration[] appliedMigrations)
	{
		MockExecuteReader<ClickHouseMigrationContext, AppliedMigration>(
			sql => sql == $"select id, name from {MigrationsDatabaseName}.{MigrationsHistoryTableName} final",
			appliedMigrations,
			("id", typeof(ulong), m => m.Index),
			("name", typeof(string), m => m.Name));
	}

	private void SetupDatabaseVersion(string version = "24.1")
	{
		MockExecuteScalar<ClickHouseMigrationContext>(sql => sql == "select version()", () => version);
	}
}
