using ClickHouse.Facades.SqlBuilders;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

internal sealed class ClickHouseMigrationFacade : ClickHouseFacade<ClickHouseMigrationContext>
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

	internal async Task EnsureMigrationsTableCreatedAsync(CancellationToken cancellationToken)
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

	internal async Task EnsureDatabaseCreatedAsync(
		CancellationToken cancellationToken)
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

	internal async Task<IEnumerable<AppliedMigration>> GetAppliedMigrationsAsync(CancellationToken cancellationToken)
	{
		var migrations = await ExecuteQueryAsync(
				GetAppliedMigrationsSql,
				AppliedMigration.FromReader,
				cancellationToken)
			.ToListAsync(cancellationToken);

		return migrations;
	}

	private const string AddAppliedMigrationSql = "insert into {0} values ({1}, '{2}', 0)";

	internal async Task ApplyMigrationAsync(ClickHouseMigration migration, CancellationToken cancellationToken)
	{
		ExceptionHelpers.ThrowIfNull(migration);

		var migrationBuilder = ClickHouseMigrationBuilder.Create;

		migration.Up(migrationBuilder);

		cancellationToken.ThrowIfCancellationRequested();

		foreach (var statement in migrationBuilder.Statements)
		{
			await ExecuteNonQueryAsync(statement, CancellationToken.None);
		}

		var addAppliedMigrationSql = string.Format(
			AddAppliedMigrationSql,
			new object[]
			{
				$"{_dbName}.{MigrationsTable}",
				migration.Index,
				migration.Name,
			});

		await ExecuteNonQueryAsync(addAppliedMigrationSql, CancellationToken.None);
	}

	private const string AddRolledBackMigrationSql = "insert into {0} values ({1}, '{2}', 1)";

	internal async Task RollbackMigrationAsync(ClickHouseMigration migration, CancellationToken cancellationToken)
	{
		ExceptionHelpers.ThrowIfNull(migration);

		var migrationBuilder = ClickHouseMigrationBuilder.Create;

		migration.Down(migrationBuilder);

		cancellationToken.ThrowIfCancellationRequested();

		foreach (var statement in migrationBuilder.Statements)
		{
			await ExecuteNonQueryAsync(statement, CancellationToken.None);
		}

		var addAppliedMigrationSql = string.Format(
			AddRolledBackMigrationSql,
			new object[]
			{
				$"{_dbName}.{MigrationsTable}",
				migration.Index,
				migration.Name,
			});

		await ExecuteNonQueryAsync(addAppliedMigrationSql, CancellationToken.None);
	}
}
