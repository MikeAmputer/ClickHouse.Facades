using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.SqlBuilders;

internal class CreateDatabaseSqlBuilder : ClickHouseSqlBuilder<CreateDatabaseSqlBuilder>
{
	private OptionalValue<bool> _ifNotExists;
	private OptionalValue<string> _dbName;
	private OptionalValue<string> _onCluster;
	private OptionalValue<string> _comment;
	private OptionalValue<ClickHouseDatabaseEngineBuilder> _engineBuilder;

	public CreateDatabaseSqlBuilder WithEngine(
		Func<ClickHouseDatabaseEngineBuilder, ClickHouseDatabaseEngineBuilder> builderSetup)
	{
		ExceptionHelpers.ThrowIfNull(builderSetup);

		var engineBuilder = builderSetup.Invoke(ClickHouseDatabaseEngineBuilder.Create);

		return WithPropertyValue(
			builder => builder._engineBuilder,
			(builder, value) => builder._engineBuilder = value,
			engineBuilder);
	}

	public CreateDatabaseSqlBuilder WithComment(string comment)
	{
		return WithPropertyValue(
			builder => builder._comment,
			(builder, value) => builder._comment = value,
			comment);
	}

	public CreateDatabaseSqlBuilder OnCluster(string onCluster)
	{
		return WithPropertyValue(
			builder => builder._onCluster,
			(builder, value) => builder._onCluster = value,
			onCluster);
	}

	public CreateDatabaseSqlBuilder WithDbName(string dbName)
	{
		return WithPropertyValue(
			builder => builder._dbName,
			(builder, value) => builder._dbName = value,
			dbName);
	}

	public CreateDatabaseSqlBuilder IfNotExists()
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
			_dbName.NotNullOrThrow(),
			_onCluster.NotNullToString(v => $" on cluster {v}", string.Empty),
			_engineBuilder.NotNullToString(v => $"\n{v.BuildSql()}", string.Empty),
			_comment.NotNullToString(v => $"\ncomment '{v}'", string.Empty),
		};
	}

	protected override string Query => "create database{0} {1}{2}{3}{4}";
}
