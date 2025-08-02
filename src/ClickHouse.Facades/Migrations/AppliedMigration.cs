using System.Data.Common;

namespace ClickHouse.Facades.Migrations;

internal class AppliedMigration
{
	public ulong Index { get; }

	public string Name { get; }

	internal AppliedMigration(ulong index, string name)
	{
		Index = index;
		Name = name;
	}

	internal static AppliedMigration FromReader(DbDataReader reader) =>
		new(reader.GetFieldValue<ulong>(0), reader.GetString(1));
}
