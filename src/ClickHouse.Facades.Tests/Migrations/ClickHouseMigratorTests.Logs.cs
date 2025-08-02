using ClickHouse.Facades.Migrations;
using Moq;

namespace ClickHouse.Facades.Tests;

public partial class ClickHouseMigratorTests
{
	[TestMethod]
	public async Task NoMigrations_LogEmpty()
	{
		const string databaseName = "test";
		SetupMigrations(databaseName, rollbackOnMigrationFail: false);
		SetupAppliedMigrations(databaseName);

		SetupDatabaseVersion();


		var migrator = GetService<IClickHouseMigrator>();
		await migrator.ApplyMigrationsAsync();


		var log = migrator.MigrationLog;

		Assert.IsTrue(log.Success);
		Assert.AreEqual(0, log.Entries.Count);
		Assert.IsNull(log.InitialMigrationIndex);
		Assert.IsNull(log.InitialMigrationName);
		Assert.IsNull(log.FinalMigrationIndex);
		Assert.IsNull(log.FinalMigrationName);
	}

	[TestMethod]
	public async Task ApplyMigrations_Success_MigrationsLogged()
	{
		const string databaseName = "test";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration"));

		SetupMigrations(databaseName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName);

		SetupDatabaseVersion();


		var migrator = GetService<IClickHouseMigrator>();
		await migrator.ApplyMigrationsAsync();


		var log = migrator.MigrationLog;

		Assert.IsTrue(log.Success);
		Assert.AreEqual(1, log.Entries.Count);
		Assert.IsNull(log.InitialMigrationIndex);
		Assert.IsNull(log.InitialMigrationName);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, log.FinalMigrationIndex);
		Assert.AreEqual(_1_FirstMigration.MigrationName, log.FinalMigrationName);

		var logEntry = log.Entries.Single();

		Assert.AreEqual(_1_FirstMigration.MigrationIndex, logEntry.Index);
		Assert.AreEqual(_1_FirstMigration.MigrationName, logEntry.Name);
		Assert.AreEqual(MigrationDirection.Up, logEntry.Direction);
		Assert.IsTrue(logEntry.Success);
		Assert.IsNull(logEntry.FailedStatement);
		Assert.IsNull(logEntry.Error);
		Assert.AreEqual(1, logEntry.ExecutedStatements.Count);
		Assert.AreEqual("apply migration", logEntry.ExecutedStatements.Single());
	}

	[TestMethod]
	public async Task ApplyMigrations_FailNoRollback_MigrationsLogged()
	{
		const string databaseName = "test";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b =>
			{
				b.AddRawSqlStatement("apply migration statement 1");
				b.AddRawSqlStatement("apply migration statement 2");
			});

		migrationMock
			.Setup(m => m.Down(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("rollback migration"));

		SetupMigrations(databaseName, rollbackOnMigrationFail: false, migrationMock.Object);
		SetupAppliedMigrations(databaseName);

		MockExecuteNonQuery<ClickHouseMigrationContext>(
			sql => sql == "apply migration statement 2",
			() => throw new Exception("test exception"));

		SetupDatabaseVersion();


		var migrator = GetService<IClickHouseMigrator>();

		await Assert.ThrowsAsync<Exception>(() => migrator.ApplyMigrationsAsync());


		var log = migrator.MigrationLog;

		Assert.IsFalse(log.Success);
		Assert.AreEqual(1, log.Entries.Count);
		Assert.IsNull(log.InitialMigrationIndex);
		Assert.IsNull(log.InitialMigrationName);
		Assert.IsNull(log.FinalMigrationIndex);
		Assert.IsNull(log.FinalMigrationName);

		var logEntry = log.Entries.Single();

		Assert.AreEqual(_1_FirstMigration.MigrationIndex, logEntry.Index);
		Assert.AreEqual(_1_FirstMigration.MigrationName, logEntry.Name);
		Assert.AreEqual(MigrationDirection.Up, logEntry.Direction);
		Assert.IsFalse(logEntry.Success);
		Assert.AreEqual("apply migration statement 2", logEntry.FailedStatement);
		Assert.AreEqual("test exception", logEntry.Error);
		Assert.AreEqual(1, logEntry.ExecutedStatements.Count);
		Assert.AreEqual("apply migration statement 1", logEntry.ExecutedStatements.Single());
	}

	[TestMethod]
	public async Task ApplyMigrations_FailWithRollback_MigrationsLogged()
	{
		const string databaseName = "test";

		var migrationMock = _1_FirstMigration.AsMock();
		migrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b =>
			{
				b.AddRawSqlStatement("apply migration statement 1");
				b.AddRawSqlStatement("apply migration statement 2");
			});

		migrationMock
			.Setup(m => m.Down(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("rollback migration"));

		SetupMigrations(databaseName, rollbackOnMigrationFail: true, migrationMock.Object);
		SetupAppliedMigrations(databaseName);

		MockExecuteNonQuery<ClickHouseMigrationContext>(
			sql => sql == "apply migration statement 2",
			() => throw new Exception("test exception"));

		SetupDatabaseVersion();


		var migrator = GetService<IClickHouseMigrator>();

		await Assert.ThrowsAsync<Exception>(() => migrator.ApplyMigrationsAsync());


		var log = migrator.MigrationLog;

		Assert.IsFalse(log.Success);
		Assert.AreEqual(2, log.Entries.Count);
		Assert.IsNull(log.InitialMigrationIndex);
		Assert.IsNull(log.InitialMigrationName);
		Assert.IsNull(log.FinalMigrationIndex);
		Assert.IsNull(log.FinalMigrationName);

		var logEntryUp = log.Entries[0];

		Assert.AreEqual(_1_FirstMigration.MigrationIndex, logEntryUp.Index);
		Assert.AreEqual(_1_FirstMigration.MigrationName, logEntryUp.Name);
		Assert.AreEqual(MigrationDirection.Up, logEntryUp.Direction);
		Assert.IsFalse(logEntryUp.Success);
		Assert.AreEqual("apply migration statement 2", logEntryUp.FailedStatement);
		Assert.AreEqual("test exception", logEntryUp.Error);
		Assert.AreEqual(1, logEntryUp.ExecutedStatements.Count);
		Assert.AreEqual("apply migration statement 1", logEntryUp.ExecutedStatements.Single());

		var logEntryDown = log.Entries[1];

		Assert.AreEqual(_1_FirstMigration.MigrationIndex, logEntryDown.Index);
		Assert.AreEqual(_1_FirstMigration.MigrationName, logEntryDown.Name);
		Assert.AreEqual(MigrationDirection.Down, logEntryDown.Direction);
		Assert.IsTrue(logEntryDown.Success);
		Assert.IsNull(logEntryDown.FailedStatement);
		Assert.IsNull(logEntryDown.Error);
		Assert.AreEqual(1, logEntryDown.ExecutedStatements.Count);
		Assert.AreEqual("rollback migration", logEntryDown.ExecutedStatements.Single());
	}

	[TestMethod]
	public async Task RollbackMigrations_Success_MigrationsLogged()
	{
		const string databaseName = "test";

		var firstMigrationMock = _1_FirstMigration.AsMock();

		var secondMigrationMock = _2_SecondMigration.AsMock();
		secondMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 2"));

		secondMigrationMock
			.Setup(m => m.Down(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("rollback migration 2"));

		SetupMigrations(
			databaseName,
			rollbackOnMigrationFail: false,
			firstMigrationMock.Object,
			secondMigrationMock.Object);

		SetupAppliedMigrations(databaseName, _1_FirstMigration.AsApplied(), _2_SecondMigration.AsApplied());

		SetupDatabaseVersion();


		var migrator = GetService<IClickHouseMigrator>();

		await migrator.RollbackAsync(_1_FirstMigration.MigrationIndex);


		var log = migrator.MigrationLog;

		Assert.IsTrue(log.Success);
		Assert.AreEqual(1, log.Entries.Count);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, log.InitialMigrationIndex);
		Assert.AreEqual(_2_SecondMigration.MigrationName, log.InitialMigrationName);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, log.FinalMigrationIndex);
		Assert.AreEqual(_1_FirstMigration.MigrationName, log.FinalMigrationName);

		var logEntry = log.Entries.Single();

		Assert.AreEqual(_2_SecondMigration.MigrationIndex, logEntry.Index);
		Assert.AreEqual(_2_SecondMigration.MigrationName, logEntry.Name);
		Assert.AreEqual(MigrationDirection.Down, logEntry.Direction);
		Assert.IsTrue(logEntry.Success);
		Assert.IsNull(logEntry.FailedStatement);
		Assert.IsNull(logEntry.Error);
		Assert.AreEqual(1, logEntry.ExecutedStatements.Count);
		Assert.AreEqual("rollback migration 2", logEntry.ExecutedStatements.Single());
	}

	[TestMethod]
	public async Task RollbackMigrations_Fail_MigrationsLogged()
	{
		const string databaseName = "test";

		var firstMigrationMock = _1_FirstMigration.AsMock();

		var secondMigrationMock = _2_SecondMigration.AsMock();
		secondMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 2"));

		secondMigrationMock
			.Setup(m => m.Down(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("rollback migration 2"));

		SetupMigrations(
			databaseName,
			rollbackOnMigrationFail: false,
			firstMigrationMock.Object,
			secondMigrationMock.Object);

		SetupAppliedMigrations(databaseName, _1_FirstMigration.AsApplied(), _2_SecondMigration.AsApplied());

		MockExecuteNonQuery<ClickHouseMigrationContext>(
			sql => sql == "rollback migration 2",
			() => throw new Exception("test exception"));

		SetupDatabaseVersion();


		var migrator = GetService<IClickHouseMigrator>();

		await Assert.ThrowsAsync<Exception>(() => migrator.RollbackAsync(_1_FirstMigration.MigrationIndex));


		var log = migrator.MigrationLog;

		Assert.IsFalse(log.Success);
		Assert.AreEqual(1, log.Entries.Count);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, log.InitialMigrationIndex);
		Assert.AreEqual(_2_SecondMigration.MigrationName, log.InitialMigrationName);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, log.FinalMigrationIndex);
		Assert.AreEqual(_2_SecondMigration.MigrationName, log.FinalMigrationName);

		var logEntry = log.Entries.Single();

		Assert.AreEqual(_2_SecondMigration.MigrationIndex, logEntry.Index);
		Assert.AreEqual(_2_SecondMigration.MigrationName, logEntry.Name);
		Assert.AreEqual(MigrationDirection.Down, logEntry.Direction);
		Assert.IsFalse(logEntry.Success);
		Assert.AreEqual("rollback migration 2", logEntry.FailedStatement);
		Assert.AreEqual("test exception", logEntry.Error);
		Assert.AreEqual(0, logEntry.ExecutedStatements.Count);
	}

	[TestMethod]
	public async Task NonEmptyAppliedMigration_ApplyNewMigration_MigrationsLogged()
	{
		const string databaseName = "test";

		var firstMigrationMock = _1_FirstMigration.AsMock();

		var secondMigrationMock = _2_SecondMigration.AsMock();
		secondMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 2"));

		SetupMigrations(
			databaseName,
			rollbackOnMigrationFail: false,
			firstMigrationMock.Object,
			secondMigrationMock.Object);

		SetupAppliedMigrations(databaseName, _1_FirstMigration.AsApplied());

		SetupDatabaseVersion();


		var migrator = GetService<IClickHouseMigrator>();

		await migrator.ApplyMigrationsAsync();


		var log = migrator.MigrationLog;

		Assert.IsTrue(log.Success);
		Assert.AreEqual(1, log.Entries.Count);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, log.InitialMigrationIndex);
		Assert.AreEqual(_1_FirstMigration.MigrationName, log.InitialMigrationName);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, log.FinalMigrationIndex);
		Assert.AreEqual(_2_SecondMigration.MigrationName, log.FinalMigrationName);

		var logEntry = log.Entries.Single();

		Assert.AreEqual(_2_SecondMigration.MigrationIndex, logEntry.Index);
		Assert.AreEqual(_2_SecondMigration.MigrationName, logEntry.Name);
		Assert.AreEqual(MigrationDirection.Up, logEntry.Direction);
		Assert.IsTrue(logEntry.Success);
		Assert.IsNull(logEntry.FailedStatement);
		Assert.IsNull(logEntry.Error);
		Assert.AreEqual(1, logEntry.ExecutedStatements.Count);
		Assert.AreEqual("apply migration 2", logEntry.ExecutedStatements.Single());
	}

	[TestMethod]
	public async Task ApplyAndRollBack_SameInitialAndFinalMigrations()
	{
		const string databaseName = "test";

		var firstMigrationMock = _1_FirstMigration.AsMock();

		var secondMigrationMock = _2_SecondMigration.AsMock();
		secondMigrationMock
			.Setup(m => m.Up(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("apply migration 2"));

		secondMigrationMock
			.Setup(m => m.Down(It.IsAny<ClickHouseMigrationBuilder>()))
			.Callback<ClickHouseMigrationBuilder>(b => b.AddRawSqlStatement("rollback migration 2"));

		SetupMigrations(
			databaseName,
			rollbackOnMigrationFail: false,
			firstMigrationMock.Object,
			secondMigrationMock.Object);

		SetupAppliedMigrations(databaseName, _1_FirstMigration.AsApplied());

		MockExecuteNonQuery<ClickHouseMigrationContext>(
			sql => sql == $"insert into {databaseName}.db_migrations_history values "
				+ $"({_2_SecondMigration.MigrationIndex}, '{_2_SecondMigration.MigrationName}', 0)",
			() =>
			{
				SetupAppliedMigrations(databaseName, _1_FirstMigration.AsApplied(), _2_SecondMigration.AsApplied());

				return 0;
			});

		SetupDatabaseVersion();


		var migrator = GetService<IClickHouseMigrator>();

		await migrator.ApplyMigrationsAsync();

		await migrator.RollbackAsync(_1_FirstMigration.MigrationIndex);


		var log = migrator.MigrationLog;

		Assert.IsTrue(log.Success);
		Assert.AreEqual(2, log.Entries.Count);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, log.InitialMigrationIndex);
		Assert.AreEqual(_1_FirstMigration.MigrationName, log.InitialMigrationName);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, log.FinalMigrationIndex);
		Assert.AreEqual(_1_FirstMigration.MigrationName, log.FinalMigrationName);
	}
}
