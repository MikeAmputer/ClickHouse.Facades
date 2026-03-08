using ClickHouse.Driver;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

public class QueryOptionsBuilder : Builder<QueryOptions, QueryOptionsBuilder>
{
	private OptionalValue<string> _database;

	public QueryOptionsBuilder WithDatabase(string database)
	{
		return WithPropertyValue(
			builder => builder._database,
			(builder, value) => builder._database = value,
			database,
			overrideAllowed: true);
	}

	protected override QueryOptions BuildCore()
	{
		return new QueryOptions
		{
			Database = _database.OrElseValue(null),
		};
	}
}
