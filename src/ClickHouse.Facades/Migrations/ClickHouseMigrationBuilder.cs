using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

public sealed class ClickHouseMigrationBuilder : IVersionedClickHouseMigrationBuilder
{
	private readonly List<string> _statements = [];
	private readonly ClickHouseVersion _clickHouseVersion;

	internal List<string> Statements => _statements;

	internal static ClickHouseMigrationBuilder Create(string clickHouseVersion) => new(clickHouseVersion);

	private ClickHouseMigrationBuilder(string clickHouseVersion)
	{
		_clickHouseVersion = new ClickHouseVersion(clickHouseVersion);
	}

	/// <summary>
	/// Executes the given migration action if the current ClickHouse version matches the provided predicate.
	/// </summary>
	/// <param name="versionPredicate">A predicate that determines whether the migration should be applied for the current ClickHouse version.</param>
	/// <param name="migrationAction">The migration action to execute if the version predicate matches.</param>
	public void WhenVersion(
		Predicate<ClickHouseVersion> versionPredicate,
		Action<IVersionedClickHouseMigrationBuilder> migrationAction)
	{
		if (versionPredicate(_clickHouseVersion))
		{
			migrationAction(this);
		}
	}

	/// <summary>
	/// Executes the migration action for a specific range of ClickHouse versions.
	/// </summary>
	/// <param name="supportedSince">The minimum supported ClickHouse version (inclusive).</param>
	/// <param name="deprecatedIn">The ClickHouse version in which support is removed (exclusive).</param>
	/// <param name="migrationAction">The migration action to execute if the current version is within the specified range.</param>
	public void ForVersionRange(
		ClickHouseVersion supportedSince,
		ClickHouseVersion deprecatedIn,
		Action<IVersionedClickHouseMigrationBuilder> migrationAction)
	{
		WhenVersion(v => v >= supportedSince && v < deprecatedIn, migrationAction);
	}

	/// <summary>
	/// Executes the migration action if the current ClickHouse version is equal to or greater than the specified version.
	/// </summary>
	/// <param name="supportedSince">The minimum ClickHouse version (inclusive) from which the migration is supported.</param>
	/// <param name="migrationAction">The migration action to execute if the current version is supported.</param>
	public void SinceVersion(
		ClickHouseVersion supportedSince,
		Action<IVersionedClickHouseMigrationBuilder> migrationAction)
	{
		WhenVersion(v => v >= supportedSince, migrationAction);
	}

	public void AddRawSqlStatement(string sql)
	{
		if (sql.IsNullOrWhiteSpace())
		{
			throw new ArgumentException("Migration statement is null or empty.", nameof(sql));
		}

		_statements.Add(sql);
	}

	public void AddSqlFileStatements(string filePath)
	{
		AddSqlFileStatements(filePath, new SemicolonSqlStatementParser());
	}

	public void AddSqlFileStatements(string filePath, ISqlStatementParser parser)
	{
		var sqlText = File.ReadAllText(filePath);

		foreach (var statement in parser.ParseStatements(sqlText))
		{
			AddRawSqlStatement(statement);
		}
	}
}
