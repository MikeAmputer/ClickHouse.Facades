namespace ClickHouse.Facades.Migrations;

public interface IMigrationFileNameParser
{
	bool TryParse(string fileName, out MigrationFileInfo? fileInfo);
}
