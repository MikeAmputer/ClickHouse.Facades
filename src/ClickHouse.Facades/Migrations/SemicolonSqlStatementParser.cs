using System.Text.RegularExpressions;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

internal sealed class SemicolonSqlStatementParser : ISqlStatementParser
{
	private static readonly Regex SplitRegex = new(@";[ \t]*(\r\n|\n|\r)");

	public IEnumerable<string> ParseStatements(string sqlText)
	{
		if (sqlText.IsNullOrWhiteSpace())
		{
			return [];
		}

		return SplitRegex
			.Split(sqlText)
			.Where((_, i) => i % 2 == 0)
			.Select(s => s.Trim().Trim(';').Trim())
			.Where(s => !s.IsNullOrWhiteSpace());
	}
}
