// ReSharper disable InconsistentNaming
namespace ClickHouse.Facades.SqlBuilders;

internal enum ClickHouseDatabaseEngine
{
	Atomic = 0,
	Lazy,
	MySQL,
	PostgresSQL,
	MaterializedMySQL,
	// ReSharper disable once IdentifierTypo
	MaterializedPostgreSQL,
	Replicated,
	SQLite,
}
