using System.Data.Common;

namespace ClickHouse.Facades.Migrations;

internal class AppliedMigration
{
	public ulong Id { get; set; }

	public string Name { get; set; }

	internal AppliedMigration(ulong id, string name)
	{
		Id = id;
		Name = name;
	}

	internal static AppliedMigration FromReader(DbDataReader reader) =>
		new(reader.GetFieldValue<ulong>(0), reader.GetString(1));
}
