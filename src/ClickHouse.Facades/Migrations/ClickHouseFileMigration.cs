namespace ClickHouse.Facades.Migrations;

internal sealed class ClickHouseFileMigration(
	ulong index,
	string name,
	ISqlStatementParser sqlStatementParser)
	: ClickHouseMigration
{
	internal override ulong Index { get; } = index;
	internal override string Name { get; } = name;

	private ISqlStatementParser SqlStatementParser { get; } = sqlStatementParser;

	private readonly ICollection<string> _upFilePaths = [];
	private readonly ICollection<string> _downFilePaths = [];

	internal void AddMigrationFile(string filePath, MigrationDirection direction)
	{
		var targetCollection = direction switch
		{
			MigrationDirection.Up => _upFilePaths,
			MigrationDirection.Down => _downFilePaths,
			_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
		};

		targetCollection.Add(filePath);
	}

	protected internal override void Up(ClickHouseMigrationBuilder migrationBuilder)
	{
		if (_upFilePaths.Count == 0)
		{
			throw new InvalidOperationException($"No up migration files found for migration {Index}_{Name}.");
		}

		foreach (var filePath in _upFilePaths)
		{
			migrationBuilder.AddSqlFileStatements(filePath, SqlStatementParser);
		}
	}

	protected internal override void Down(ClickHouseMigrationBuilder migrationBuilder)
	{
		if (_downFilePaths.Count == 0)
		{
			throw new InvalidOperationException($"No down migration files found for migration {Index}_{Name}.");
		}

		foreach (var filePath in _downFilePaths)
		{
			migrationBuilder.AddSqlFileStatements(filePath, SqlStatementParser);
		}
	}
}
