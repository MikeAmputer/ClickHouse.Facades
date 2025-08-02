namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrationLogEntry
{
	ulong Index { get; }
	string Name { get; }
	MigrationDirection Direction { get; }
	IReadOnlyList<string> ExecutedStatements { get; }
	bool Success { get; }
	string? FailedStatement { get; set; }
	string? Error { get; }
}
