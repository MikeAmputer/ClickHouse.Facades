namespace ClickHouse.Facades.Migrations;

public class MigrationFileInfo
{
	public ulong Index { get; set; }
	public string Name { get; set; } = string.Empty;
	public MigrationDirection Direction { get; set; }
}
