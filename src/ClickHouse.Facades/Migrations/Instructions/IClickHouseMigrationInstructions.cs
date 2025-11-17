using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

/// <summary>
/// Provides instructions for configuring ClickHouse database migrations.
/// </summary>
public interface IClickHouseMigrationInstructions
{
	/// <summary>
	/// Gets the connection string used for migrations.
	/// </summary>
	string ConnectionString { get; }

	/// <summary>
	/// Database for migrations history table. Database will be created if not exists with engine Atomic.
	/// Gets 'database' connection string parameter value as default.
	/// </summary>
	string DatabaseName => ConnectionString.GetConnectionStringParameters()["database"]
		?? throw new InvalidOperationException("Unable to get 'database' parameter from connection string.");

	/// <summary>
	/// Migrations history table name.
	/// The default is <c>db_migrations_history</c>.
	/// </summary>
	string HistoryTableName => "db_migrations_history";

	/// <summary>
	/// Cluster name where the migrations history table is stored.
	/// Ignored by default.
	/// </summary>
	string? ClusterName => null;

	/// <summary>
	/// Replication settings for the migrations history table.
	/// If specified, the history table will be created as a <c>ReplicatedMergeTree</c>
	/// using the provided ZooKeeper path and replica name.
	/// If <c>null</c>, a non-replicated engine will be used instead.
	/// </summary>
	ClickHouseReplicatedTableArgs? ReplicatedHistoryTableArgs => null;

	/// <summary>
	/// A value indicating whether to roll back changes if a migration fails.
	/// The default is <c>false</c>.
	/// </summary>
	bool RollbackOnMigrationFail => false;

	/// <summary>
	/// Optional <see cref="HttpClient"/> used to execute migration statements.
	/// If provided, it will not be disposed internally. It is recommended to use an instance
	/// created via <see cref="IHttpClientFactory"/> to avoid lifecycle and socket issues.
	/// </summary>
	HttpClient? HttpClient => null;
}
