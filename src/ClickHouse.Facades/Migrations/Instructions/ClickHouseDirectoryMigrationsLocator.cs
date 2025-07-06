namespace ClickHouse.Facades.Migrations;

public abstract class ClickHouseDirectoryMigrationsLocator : IClickHouseMigrationsLocator
{
	protected abstract string DirectoryPath { get; }

	protected virtual IMigrationFileNameParser FileNameParser => new DefaultMigrationFileNameParser();
	protected virtual ISqlStatementParser SqlStatementParser => new SemicolonSqlStatementParser();

	public IEnumerable<ClickHouseMigration> GetMigrations()
	{
		if (!Directory.Exists(DirectoryPath))
		{
			throw new InvalidOperationException($"Migrations directory '{DirectoryPath}' does not exist.");
		}

		var migrations = new Dictionary<(ulong index, string name), ClickHouseFileMigration>();

		foreach (var filePath in Directory.EnumerateFiles(DirectoryPath))
		{
			if (!FileNameParser.TryParse(Path.GetFileName(filePath), out var fileInfo) || fileInfo == null)
			{
				continue;
			}

			if (migrations.TryGetValue((fileInfo.Index, fileInfo.Name), out var migration))
			{
				migration.AddMigrationFile(filePath, fileInfo.Direction);

				continue;
			}

			var newMigration = new ClickHouseFileMigration(fileInfo.Index, fileInfo.Name, SqlStatementParser);
			newMigration.AddMigrationFile(filePath, fileInfo.Direction);

			migrations.Add((fileInfo.Index, fileInfo.Name), newMigration);
		}

		return migrations.Select(m => m.Value);
	}
}
