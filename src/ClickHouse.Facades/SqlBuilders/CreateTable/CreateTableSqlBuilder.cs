using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.SqlBuilders;

internal class CreateTableSqlBuilder : ClickHouseSqlBuilder<CreateTableSqlBuilder>
{
	private OptionalValue<bool> _ifNotExists;
	private OptionalValue<string> _database;
	private OptionalValue<string> _tableName;
	private OptionalValue<string> _onCluster;
	private OptionalValue<string> _comment;
	private OptionalValue<ClickHouseTableEngineBuilder> _engineBuilder;
	private readonly List<string> _columns = new();
	private readonly List<string> _dataSkippingIndices = new();
	private readonly Dictionary<string, string> _constraints = new();
	private OptionalValue<string[]> _partitionBy;
	private OptionalValue<string[]> _primaryKey;
	private OptionalValue<string[]> _orderBy;

	public CreateTableSqlBuilder WithOrderBy(params string[] orderBy)
	{
		return WithPropertyValue(
			builder => builder._orderBy,
			(builder, value) => builder._orderBy = value,
			orderBy);
	}

	public CreateTableSqlBuilder WithPrimaryKey(params string[] primaryKey)
	{
		return WithPropertyValue(
			builder => builder._primaryKey,
			(builder, value) => builder._primaryKey = value,
			primaryKey);
	}

	public CreateTableSqlBuilder WithPartitionBy(params string[] partitionBy)
	{
		return WithPropertyValue(
			builder => builder._partitionBy,
			(builder, value) => builder._partitionBy = value,
			partitionBy);
	}

	public CreateTableSqlBuilder AddConstraint(string name, string expression)
	{
		return WithNamedArgument(
			builder => builder._constraints,
			name,
			expression);
	}

	public CreateTableSqlBuilder AddDataSkippingIndex(
		Func<ClickHouseDataSkippingIndexBuilder, ClickHouseDataSkippingIndexBuilder> builderSetup)
	{
		ExceptionHelpers.ThrowIfNull(builderSetup);
		ThrowIfBuilt();

		var indexBuilder = builderSetup.Invoke(ClickHouseDataSkippingIndexBuilder.Create);

		_dataSkippingIndices.Add(indexBuilder.BuildSql());

		return this;
	}

	public CreateTableSqlBuilder AddColumn(
		Func<ClickHouseColumnBuilder, ClickHouseColumnBuilder> builderSetup)
	{
		ExceptionHelpers.ThrowIfNull(builderSetup);
		ThrowIfBuilt();

		var columnBuilder = builderSetup.Invoke(ClickHouseColumnBuilder.Create);

		_columns.Add(columnBuilder.BuildSql());

		return this;
	}

	public CreateTableSqlBuilder WithEngine(
		Func<ClickHouseTableEngineBuilder, ClickHouseTableEngineBuilder> builderSetup)
	{
		ExceptionHelpers.ThrowIfNull(builderSetup);

		var engineBuilder = builderSetup.Invoke(ClickHouseTableEngineBuilder.Create);

		return WithPropertyValue(
			builder => builder._engineBuilder,
			(builder, value) => builder._engineBuilder = value,
			engineBuilder);
	}

	public CreateTableSqlBuilder WithComment(string comment)
	{
		return WithPropertyValue(
			builder => builder._comment,
			(builder, value) => builder._comment = value,
			comment);
	}

	public CreateTableSqlBuilder WithOnCluster(string onCluster)
	{
		return WithPropertyValue(
			builder => builder._onCluster,
			(builder, value) => builder._onCluster = value,
			onCluster);
	}

	public CreateTableSqlBuilder WithTableName(string tableName)
	{
		return WithPropertyValue(
			builder => builder._tableName,
			(builder, value) => builder._tableName = value,
			tableName);
	}

	public CreateTableSqlBuilder WithDatabase(string database)
	{
		return WithPropertyValue(
			builder => builder._database,
			(builder, value) => builder._database = value,
			database);
	}

	public CreateTableSqlBuilder IfNotExists()
	{
		return WithPropertyValue(
			builder => builder._ifNotExists,
			(builder, value) => builder._ifNotExists = value,
			true);
	}

	protected override object[] BuildQueryArgs()
	{
		return new object[]
		{
			_ifNotExists.ToString(_ => " if not exists"),
			_database.NotNullToString(v => $"{v}.", ""),
			_tableName.NotNullOrThrow(),
			_onCluster.NotNullToString(v => $" on cluster {v}", string.Empty),
			FetchColumns(),
			_engineBuilder.NotNullToString(v => $"\n{v.BuildSql()}", string.Empty),
			_partitionBy.NotNullToString(v => FetchColumnArgsLine("partition by", v), string.Empty),
			_primaryKey.NotNullToString(v => FetchColumnArgsLine("primary key", v), string.Empty),
			_orderBy.NotNullToString(v => FetchColumnArgsLine("order by", v), string.Empty),
			_comment.NotNullToString(v => $"\ncomment '{v}'", string.Empty),
		};
	}

	private string FetchColumns()
	{
		if (!_columns.Any())
		{
			throw new InvalidOperationException("Create table statement contains no columns.");
		}

		return string.Join(
			",\n",
			_columns
				.Union(_dataSkippingIndices)
				.Union(_constraints.Select(c => $"constraint {c.Key} check {c.Value}"))
				.Select(v => $"\t{v}"));
	}

	private string FetchColumnArgsLine(string prefix, string[] arr)
	{
		return $"\n{prefix} {arr.ToClickHouseBracedArguments()}";
	}

	// 0 - if not exists
	// 1 - database
	// 2 - table name
	// 3 - on cluster
	// 4 - columns, data skipping indices, constraints
	// 5 - engine
	// 6 - partition by
	// 7 - primary key
	// 8 - order by
	// 9 - comment
	protected override string Query => "create table{0} {1}{2}{3} (\n{4}\n){5}{6}{7}{8}{9}";
}
