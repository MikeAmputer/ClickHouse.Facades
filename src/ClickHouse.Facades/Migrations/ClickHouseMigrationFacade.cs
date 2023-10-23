using ClickHouse.Facades.SqlBuilders;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

internal sealed class ClickHouseMigrationFacade : ClickHouseFacade<ClickHouseMigrationContext>
{
	private const string MigrationsTable = "db_migrations_history";
	internal string DbName { get; set; } = string.Empty;

	internal async Task EnsureMigrationsTableCreatedAsync(CancellationToken cancellationToken)
	{
		ThrowIfDatabaseNotSet();

		var builder = CreateTableSqlBuilder.Create
			.IfNotExists()
			.WithDatabase(DbName)
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
		ThrowIfDatabaseNotSet();

		var builder = CreateDatabaseSqlBuilder.Create
			.IfNotExists()
			.WithDbName(DbName)
			.WithEngine(builder => builder.WithEngine(ClickHouseDatabaseEngine.Atomic));

		var statement = builder.BuildSql();

		await ExecuteNonQueryAsync(statement, cancellationToken);
	}

	private string GetAppliedMigrationsSql =>
		$"select id, name from {DbName}.{MigrationsTable} final";

	internal async Task<IEnumerable<AppliedMigration>> GetAppliedMigrationsAsync(CancellationToken cancellationToken)
	{
		ThrowIfDatabaseNotSet();

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
		ThrowIfDatabaseNotSet();
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
				$"{DbName}.{MigrationsTable}",
				migration.Index,
				migration.Name,
			});

		await ExecuteNonQueryAsync(addAppliedMigrationSql, CancellationToken.None);
	}

	private const string AddRolledBackMigrationSql = "insert into {0} values ({1}, '{2}', 1)";

	internal async Task RollbackMigrationAsync(ClickHouseMigration migration, CancellationToken cancellationToken)
	{
		ThrowIfDatabaseNotSet();
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
				$"{DbName}.{MigrationsTable}",
				migration.Index,
				migration.Name,
			});

		await ExecuteNonQueryAsync(addAppliedMigrationSql, CancellationToken.None);
	}

	private void ThrowIfDatabaseNotSet()
	{
		if (DbName.IsNullOrWhiteSpace())
		{
			throw new InvalidOperationException($"{nameof(DbName)} is not set.");
		}
	}
}
