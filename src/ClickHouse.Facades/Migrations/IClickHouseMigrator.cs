namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrator
{
	public Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Rolls back migrations one by one from last to first until the last applied migration has specified index.
	/// </summary>
	public Task RollbackAsync(ulong targetMigrationId, CancellationToken cancellationToken = default);

	public IClickHouseMigrationLog MigrationLog { get; }
}

public interface IClickHouseMigrator<TContext> : IClickHouseMigrator
	where TContext : ClickHouseContext<TContext>
{

}
