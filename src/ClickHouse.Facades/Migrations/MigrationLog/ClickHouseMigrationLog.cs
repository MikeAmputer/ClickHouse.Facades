namespace ClickHouse.Facades.Migrations;

internal sealed class ClickHouseMigrationLog : IClickHouseMigrationLog
{
	internal List<ClickHouseMigrationLogEntry> EntriesInternal { get; } = [];
	public IReadOnlyList<IClickHouseMigrationLogEntry> Entries => EntriesInternal;

	public ulong? InitialMigrationIndex { get; set; }
	public string? InitialMigrationName { get; set; }

	public ulong? FinalMigrationIndex { get; set; }
	public string? FinalMigrationName { get; set; }

	public bool Success => Entries.All(e => e.Success);
}
