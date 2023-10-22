namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrator
{
	public Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);
}
