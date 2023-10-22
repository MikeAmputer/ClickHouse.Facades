using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.SqlBuilders;

internal class ClickHouseDataSkippingIndexBuilder : ClickHouseSqlBuilder<ClickHouseDataSkippingIndexBuilder>
{
	private OptionalValue<string> _name;
	private OptionalValue<string> _expression;
	private OptionalValue<string> _type;
	private OptionalValue<uint> _granularity;

	public ClickHouseDataSkippingIndexBuilder WithGranularity(uint granularity)
	{
		return WithPropertyValue(
			builder => builder._granularity,
			(builder, value) => builder._granularity = value,
			granularity);
	}

	public ClickHouseDataSkippingIndexBuilder WithType(string type)
	{
		return WithPropertyValue(
			builder => builder._type,
			(builder, value) => builder._type = value,
			type);
	}

	public ClickHouseDataSkippingIndexBuilder WithExpression(string expression)
	{
		return WithPropertyValue(
			builder => builder._expression,
			(builder, value) => builder._expression = value,
			expression);
	}

	public ClickHouseDataSkippingIndexBuilder WithName(string name)
	{
		return WithPropertyValue(
			builder => builder._name,
			(builder, value) => builder._name = value,
			name);
	}

	protected override object[] BuildQueryArgs()
	{
		return new object[]
		{
			_name.NotNullOrThrow(),
			_expression.NotNullOrThrow(),
			_type.NotNullOrThrow(),
			_granularity.NotNullToString(v => v.ToString(), "1"),
		};
	}

	protected override string Query => "index {0} {1} type {2} granularity {3}";
}
