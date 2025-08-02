namespace ClickHouse.Facades.Migrations;

internal sealed class ClickHouseMigrationLogEntry : IClickHouseMigrationLogEntry
{
	public ulong Index { get; set; }
	public string Name { get; set; } = string.Empty;
	public MigrationDirection Direction { get; set; }
	public bool Success { get; set; }
	public string? FailedStatement { get; set; }
	public string? Error { get; set; }

	internal List<string> ExecutedStatementsInternal { get; } = [];
	public IReadOnlyList<string> ExecutedStatements => ExecutedStatementsInternal;
}
