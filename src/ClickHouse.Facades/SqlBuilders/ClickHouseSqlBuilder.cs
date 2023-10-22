using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.SqlBuilders;

internal abstract class ClickHouseSqlBuilder<TBuilder> : Builder<string, TBuilder>
	where TBuilder : ClickHouseSqlBuilder<TBuilder>, new()
{
	protected sealed override string BuildCore()
	{
		return string.Format(Query, BuildQueryArgs());
	}

	internal string BuildSql()
	{
		return BuildCore();
	}

	protected abstract object[] BuildQueryArgs();
	protected abstract string Query { get; }
}
