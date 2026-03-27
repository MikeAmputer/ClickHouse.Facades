namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrationLog
{
	IReadOnlyList<IClickHouseMigrationLogEntry> Entries { get; }

	ulong? InitialMigrationIndex { get; }
	string? InitialMigrationName { get; }

	ulong? FinalMigrationIndex { get; }
	string? FinalMigrationName { get; }

	bool Success { get; }
}
