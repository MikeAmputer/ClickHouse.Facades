namespace ClickHouse.Facades.SqlBuilders;

internal enum ClickHouseTableEngine
{
	MergeTree = 0,
	ReplacingMergeTree,
	SummingMergeTree,
	AggregatingMergeTree,
	CollapsingMergeTree,
	VersionedCollapsingMergeTree,
	GraphiteMergeTree,
	TinyLog,
	StripeLog,
	Log,
}
