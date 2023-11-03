using ClickHouse.Facades.Migrations;
using ClickHouse.Facades.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ClickHouse.Facades.Tests;

[TestClass]
public class ClickHouseContextMigratorTests : ClickHouseFacadesTestsCore
{
	private const string MigrationsDatabaseName = "test";

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

		services.AddClickHouseTestMigrations(migrationInstructionsMock.Object);
	}

	[TestMethod]
	public async Task NoMigrations_ApplySingleContextMigrations_MigrationsTableCreated()
	{
		SetupMigrations<TestContext_1>(Enumerable.Empty<ClickHouseMigration>().ToArray());
		SetupAppliedMigrations(Enumerable.Empty<AppliedMigration>().ToArray());


		await GetService<IClickHouseMigrator<TestContext_1>>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(3, connectionTracker.RecordsCount);

		Assert.AreEqual(
			$"create database if not exists {MigrationsDatabaseName}\nengine = Atomic",
			connectionTracker.GetRecord(1).Sql);

		Assert.IsTrue(
			connectionTracker
				.GetRecord(2)
				.Sql
				.StartsWith($"create table if not exists {MigrationsDatabaseName}.db_migrations_history"));
	}

	[TestMethod]
	public async Task OneMigrationToApply_ApplySingleContextMigrations_MigrationApplied()
	{
		Mock<_1_FirstMigration> migrationMock = new();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration"));

		SetupMigrations<TestContext_1>(migrationMock.Object);
		SetupAppliedMigrations(Enumerable.Empty<AppliedMigration>().ToArray());


		await GetService<IClickHouseMigrator<TestContext_1>>().ApplyMigrationsAsync();


		var connectionTracker = GetClickHouseConnectionTracker<ClickHouseMigrationContext>();

		Assert.AreEqual(5, connectionTracker.RecordsCount);

		Assert.AreEqual("apply migration", connectionTracker.GetRecord(4).Sql);

		Assert.AreEqual(
			$"insert into {MigrationsDatabaseName}.db_migrations_history values "
			+ $"({_1_FirstMigration.MigrationIndex}, '{_1_FirstMigration.MigrationName}', 0)",
			connectionTracker.GetRecord(5).Sql);
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
			sql => sql == $"select id, name from {MigrationsDatabaseName}.db_migrations_history final",
			appliedMigrations,
			("id", typeof(ulong), m => m.Id),
			("name", typeof(string), m => m.Name));
	}
}
