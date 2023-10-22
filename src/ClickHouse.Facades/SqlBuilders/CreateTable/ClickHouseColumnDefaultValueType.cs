namespace ClickHouse.Facades.SqlBuilders;

internal enum ClickHouseColumnDefaultValueType
{
	Default = 0,
	Materialized,
	Ephemeral,
	Alias,
}
