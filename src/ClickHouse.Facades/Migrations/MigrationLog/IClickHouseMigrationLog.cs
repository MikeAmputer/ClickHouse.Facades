namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrationLog
{
	IReadOnlyList<IClickHouseMigrationLogEntry> Entries { get; }

	public ulong? InitialMigrationIndex { get; }
	public string? InitialMigrationName { get; }

	public ulong? FinalMigrationIndex { get; }
	public string? FinalMigrationName { get; }

	bool Success { get; }
}
