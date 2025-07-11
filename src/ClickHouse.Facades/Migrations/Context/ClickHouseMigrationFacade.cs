﻿using ClickHouse.Facades.SqlBuilders;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

internal sealed class ClickHouseMigrationFacade
	: ClickHouseFacade<ClickHouseMigrationContext>, IClickHouseMigrationFacade
{
	private const string MigrationsTable = "db_migrations_history";

	private readonly IClickHouseMigrationInstructions _migrationInstructions;
	private readonly string _dbName;

	public ClickHouseMigrationFacade(IClickHouseMigrationInstructions migrationInstructions)
	{
		_migrationInstructions = migrationInstructions
			?? throw new ArgumentNullException(nameof(migrationInstructions));

		if (_migrationInstructions.DatabaseName.IsNullOrWhiteSpace())
		{
			throw new ArgumentException($"Migrations database name is null or white space.");
		}

		_dbName = _migrationInstructions.DatabaseName;
	}

	public async Task EnsureMigrationsTableCreatedAsync(CancellationToken cancellationToken)
	{
		var builder = CreateTableSqlBuilder.Create
			.IfNotExists()
			.WithDatabase(_dbName)
			.WithTableName(MigrationsTable)
			.AddColumn(builder => builder
				.WithName("id")
				.WithType("UInt64"))
			.AddColumn(builder => builder
				.WithName("name")
				.WithType("String"))
			.AddColumn(builder => builder
				.WithName("date_time")
				.WithType("DateTime('UTC')")
				.WithDefaultValue(ClickHouseColumnDefaultValueType.Materialized, "now('UTC')"))
			.AddColumn(builder => builder
				.WithName("is_rolled_back")
				.WithType("UInt8"))
			.WithEngine(builder => builder
				.WithEngine(ClickHouseTableEngine.ReplacingMergeTree)
				.WithEngineArgs("date_time", "is_rolled_back"))
			.WithOrderBy("id");

		var statement = builder.BuildSql();

		await ExecuteNonQueryAsync(statement, cancellationToken);
	}

	public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken)
	{
		var builder = CreateDatabaseSqlBuilder.Create
			.IfNotExists()
			.WithDbName(_dbName)
			.WithEngine(builder => builder.WithEngine(ClickHouseDatabaseEngine.Atomic));

		var statement = builder.BuildSql();

		await ExecuteNonQueryAsync(statement, cancellationToken);
	}

	private string GetAppliedMigrationsSql =>
		$"select id, name from {_dbName}.{MigrationsTable} final";

	public async Task<List<AppliedMigration>> GetAppliedMigrationsAsync(CancellationToken cancellationToken)
	{
		var migrations = await ExecuteQueryAsync(
				GetAppliedMigrationsSql,
				AppliedMigration.FromReader,
				cancellationToken)
			.ToListAsync(cancellationToken);

		return migrations;
	}

	private const string AddAppliedMigrationSql = "insert into {0} values ({1}, '{2}', 0)";

	public async Task ApplyMigrationAsync(ClickHouseMigration migration, CancellationToken cancellationToken)
	{
		ExceptionHelpers.ThrowIfNull(migration);

		var dbVersion = await GetDatabaseVersion(cancellationToken);

		var migrationBuilder = ClickHouseMigrationBuilder.Create(dbVersion);

		migration.Up(migrationBuilder);

		cancellationToken.ThrowIfCancellationRequested();

		var statementsExecuted = 0;

		try
		{
			foreach (var statement in migrationBuilder.Statements)
			{
				await ExecuteNonQueryAsync(statement, CancellationToken.None);

				statementsExecuted++;
			}

			var addAppliedMigrationSql = string.Format(
				AddAppliedMigrationSql,
				[
					$"{_dbName}.{MigrationsTable}",
					migration.Index,
					migration.Name
				]);

			await ExecuteNonQueryAsync(addAppliedMigrationSql, CancellationToken.None);
		}
		catch (Exception migrationException)
		{
			var rolledBack = await TryRollbackMigrationAsync(migration);
			var verb = rolledBack ? "has been" : "has not been";

			throw new AggregateException(
				$"Failed to apply migration '{migration.Name}'. " +
				$"Successfully executed statements : {statementsExecuted}. " +
				$"Migration {verb} rolled back.",
				migrationException);
		}
	}

	private const string AddRolledBackMigrationSql = "insert into {0} values ({1}, '{2}', 1)";

	public async Task RollbackMigrationAsync(ClickHouseMigration migration, CancellationToken cancellationToken)
	{
		ExceptionHelpers.ThrowIfNull(migration);

		var dbVersion = await GetDatabaseVersion(cancellationToken);

		var migrationBuilder = ClickHouseMigrationBuilder.Create(dbVersion);

		migration.Down(migrationBuilder);

		cancellationToken.ThrowIfCancellationRequested();

		foreach (var statement in migrationBuilder.Statements)
		{
			await ExecuteNonQueryAsync(statement, CancellationToken.None);
		}

		var addAppliedMigrationSql = string.Format(
			AddRolledBackMigrationSql,
			[
				$"{_dbName}.{MigrationsTable}",
				migration.Index,
				migration.Name
			]);

		await ExecuteNonQueryAsync(addAppliedMigrationSql, CancellationToken.None);
	}

	private async Task<bool> TryRollbackMigrationAsync(ClickHouseMigration migration)
	{
		if (!_migrationInstructions.RollbackOnMigrationFail)
		{
			return false;
		}

		try
		{
			await RollbackMigrationAsync(migration, CancellationToken.None);

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private string? _dbVersion = null;

	private async Task<string> GetDatabaseVersion(CancellationToken cancellationToken)
	{
		_dbVersion ??= await ExecuteScalarAsync<string>("select version()", cancellationToken);

		return _dbVersion;
	}
}
