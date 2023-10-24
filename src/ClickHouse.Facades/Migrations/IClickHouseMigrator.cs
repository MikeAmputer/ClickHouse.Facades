namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrator
{
	public Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);

	public Task RollbackAsync(ulong targetMigrationId, CancellationToken cancellationToken = default);
}
