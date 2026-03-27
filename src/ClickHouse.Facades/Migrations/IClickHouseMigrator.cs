namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrator
{
	Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Rolls back migrations one by one from last to first until the last applied migration has specified index.
	/// </summary>
	Task RollbackAsync(ulong targetMigrationId, CancellationToken cancellationToken = default);

	IClickHouseMigrationLog MigrationLog { get; }
}

public interface IClickHouseMigrator<TContext> : IClickHouseMigrator
	where TContext : ClickHouseContext<TContext>
{

}
