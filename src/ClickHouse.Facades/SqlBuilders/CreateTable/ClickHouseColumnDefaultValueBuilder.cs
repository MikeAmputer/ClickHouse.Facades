using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.SqlBuilders;

internal class ClickHouseColumnDefaultValueBuilder : ClickHouseSqlBuilder<ClickHouseColumnDefaultValueBuilder>
{
	private OptionalValue<ClickHouseColumnDefaultValueType> _type;
	private OptionalValue<string> _expression;

	public ClickHouseColumnDefaultValueBuilder WithExpression(string expression)
	{
		return WithPropertyValue(
			builder => builder._expression,
			(builder, value) => builder._expression = value,
			expression);
	}

	public ClickHouseColumnDefaultValueBuilder WithType(ClickHouseColumnDefaultValueType type)
	{
		return WithPropertyValue(
			builder => builder._type,
			(builder, value) => builder._type = value,
			type);
	}

	protected override object[] BuildQueryArgs()
	{
		return new object[]
		{
			_type.NotNullToString(v => v.ToString().ToLower()),
			_expression.NotNullOrThrow(),
		};
	}

	protected override string Query => "{0} {1}";
}
