using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.SqlBuilders;

internal class ClickHouseColumnBuilder : ClickHouseSqlBuilder<ClickHouseColumnBuilder>
{
	private OptionalValue<string> _name;
	private OptionalValue<string> _type;
	private OptionalValue<bool> _nullable;
	private OptionalValue<ClickHouseColumnDefaultValueType> _defaultValueType;
	private OptionalValue<string> _defaultValueExpr;
	private OptionalValue<string> _compressionCodec;
	private OptionalValue<string> _ttl;
	private OptionalValue<string> _comment;

	public ClickHouseColumnBuilder WithComment(string comment)
	{
		return WithPropertyValue(
			builder => builder._comment,
			(builder, value) => builder._comment = value,
			comment);
	}

	public ClickHouseColumnBuilder WithTtl(string ttl)
	{
		return WithPropertyValue(
			builder => builder._ttl,
			(builder, value) => builder._ttl = value,
			ttl);
	}

	public ClickHouseColumnBuilder WithCompressionCodec(string compressionCodec)
	{
		return WithPropertyValue(
			builder => builder._compressionCodec,
			(builder, value) => builder._compressionCodec = value,
			compressionCodec);
	}

	public ClickHouseColumnBuilder WithDefaultValue(
		ClickHouseColumnDefaultValueType type,
		string expression)
	{
		WithPropertyValue(
			builder => builder._defaultValueType,
			(builder, value) => builder._defaultValueType = value,
			type);

		return WithPropertyValue(
			builder => builder._defaultValueExpr,
			(builder, value) => builder._defaultValueExpr = value,
			expression);
	}

	public ClickHouseColumnBuilder NotNullable()
	{
		return WithPropertyValue(
			builder => builder._nullable,
			(builder, value) => builder._nullable = value,
			false);
	}

	public ClickHouseColumnBuilder Nullable()
	{
		return WithPropertyValue(
			builder => builder._nullable,
			(builder, value) => builder._nullable = value,
			true);
	}

	public ClickHouseColumnBuilder WithType(string type)
	{
		return WithPropertyValue(
			builder => builder._type,
			(builder, value) => builder._type = value,
			type);
	}

	public ClickHouseColumnBuilder WithName(string name)
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
			_type.NotNullOrThrow(),
			_nullable.ToString(v => $" {(v ? "null" : "not null")}"),
			_defaultValueType.NotNullToString(
				t => $" {t.ToString().ToLower()} {_defaultValueExpr.NotNullOrThrow()}",
				string.Empty),
			_compressionCodec.NotNullToString(v => $" {v}", string.Empty),
			_ttl.NotNullToString(v => $" ttl {v}", string.Empty),
			_comment.NotNullToString(v => $" comment '{v}'", string.Empty),
		};
	}

	protected override string Query => "{0} {1}{2}{3}{4}{5}{6}";
}
