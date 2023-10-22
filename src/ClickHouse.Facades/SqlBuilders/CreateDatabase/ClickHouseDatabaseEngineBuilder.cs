using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.SqlBuilders;

internal class ClickHouseDatabaseEngineBuilder : ClickHouseSqlBuilder<ClickHouseDatabaseEngineBuilder>
{
	private OptionalValue<ClickHouseDatabaseEngine> _engine;
	private OptionalValue<string[]> _engineArgs;
	private readonly Dictionary<string, string> _settings = new();

	public ClickHouseDatabaseEngineBuilder WithEngine(ClickHouseDatabaseEngine engine)
	{
		return WithPropertyValue(
			builder => builder._engine,
			(builder, value) => builder._engine = value,
			engine);
	}

	public ClickHouseDatabaseEngineBuilder WithEngineArgs(params string[] args)
	{
		return WithPropertyValue(
			builder => builder._engineArgs,
			(builder, value) => builder._engineArgs = value,
			args);
	}

	public ClickHouseDatabaseEngineBuilder AddEngineSettings(string key, string value)
	{
		return WithNamedArgument(
			builder => builder._settings,
			key,
			value);
	}

	protected override object[] BuildQueryArgs()
	{
		return new object[]
		{
			_engine.NotNullToString(v => v.ToString()),
			_engineArgs.NotNullToString(v => v.ToClickHouseBracedArguments(), string.Empty),
			_settings.Any() ? $"\n{_settings.ToClickHouseSettings()}" : string.Empty,
		};
	}

	protected override string Query => "engine = {0}{1}{2}";
}
