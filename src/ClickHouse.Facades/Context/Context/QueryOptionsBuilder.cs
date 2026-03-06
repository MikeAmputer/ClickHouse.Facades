using ClickHouse.Driver;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

public class QueryOptionsBuilder : Builder<QueryOptions, QueryOptionsBuilder>
{
	private OptionalValue<string> _database;
	private Dictionary<string, object>? _customSettings;

	public QueryOptionsBuilder WithDatabase(string database)
	{
		return WithPropertyValue(
			builder => builder._database,
			(builder, value) => builder._database = value,
			database,
			overrideAllowed: true);
	}

	public QueryOptionsBuilder AddCustomSettings(string parameterName, object value)
	{
		_customSettings ??= new();

		_customSettings.Add(parameterName, value);

		return this;
	}

	protected override QueryOptions BuildCore()
	{
		return new QueryOptions
		{
			Database = _database.OrElseValue(null),
			CustomSettings = _customSettings,
		};
	}
}
