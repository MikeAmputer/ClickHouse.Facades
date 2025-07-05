using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Tests;

[TestClass]
public class SemicolonSqlStatementParserTests
{
	private SemicolonSqlStatementParser _parser = null!;

	[TestInitialize]
	public void Setup()
	{
		_parser = new SemicolonSqlStatementParser();
	}

	[TestMethod]
	public void ParseStatements_NullInput_ReturnsEmpty()
	{
		var result = _parser.ParseStatements(null!);

		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void ParseStatements_EmptyInput_ReturnsEmpty()
	{
		var result = _parser.ParseStatements("");

		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void ParseStatements_SemicolonAndLineBreak_ReturnsEmpty()
	{
		var result = _parser.ParseStatements(";\n");

		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void ParseStatements_WhitespaceOnly_ReturnsEmpty()
	{
		var result = _parser.ParseStatements("   \n\t\r  ");

		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void ParseStatements_SingleStatementWithoutSemicolon_ReturnsSingle()
	{
		var result = _parser.ParseStatements("SELECT 1");

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1"
			},
			result.ToList());
	}

	[TestMethod]
	public void ParseStatements_StatementEndsWithoutLineBreak_ReturnsSingle()
	{
		var result = _parser.ParseStatements("SELECT 1;");

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_StatementEndsWithSemicolonAndNewLine_ReturnsSingle()
	{
		var result = _parser.ParseStatements("SELECT 1;\n");

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_NewLineBeforeAndAfterSemicolon_TrimsNewLines()
	{
		var result = _parser.ParseStatements("SELECT 1\n;\n");

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_NewLineOnlyBeforeSemicolon_TrimsNewLine()
	{
		var result = _parser.ParseStatements("SELECT 1\n;");

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_StatementEndsWithMultipleSemicolons_ReturnsSingle()
	{
		var result = _parser.ParseStatements("SELECT 1;;;");

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_MultipleLineBreaks_TrimsLineBreaks()
	{
		const string input = "\n\nSELECT 1;\n\n\n";

		var result = _parser.ParseStatements(input);

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_SemicolonInsideStringLiteral_DoesNotSplit()
	{
		const string input = "SELECT 'value; part of string';";

		var result = _parser.ParseStatements(input);

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 'value; part of string'"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_TwoStatementsSeparatedBySemicolonAndNewline_ReturnsBoth()
	{
		const string input = "SELECT 1;\nSELECT 2;";

		var result = _parser.ParseStatements(input);

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1",
				"SELECT 2"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_TwoStatementsSeparatedBySemicolonAndMultipleNewlines_ReturnsBoth()
	{
		const string input = "SELECT 1;\n\n\nSELECT 2;";

		var result = _parser.ParseStatements(input);

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1",
				"SELECT 2"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_HandlesDifferentLineEndings()
	{
		const string input = "SELECT 1;\r\nSELECT 2;\rSELECT 3;\nSELECT 4;";

		var result = _parser.ParseStatements(input);

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1",
				"SELECT 2",
				"SELECT 3",
				"SELECT 4"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_ExtraWhitespaceAfterSemicolon_TrimsWhitespaces()
	{
		const string input = "SELECT 1;   \nSELECT 2; \t\rSELECT 3;";

		var result = _parser.ParseStatements(input);

		CollectionAssert.AreEqual(
			new List<string>
			{
				"SELECT 1",
				"SELECT 2",
				"SELECT 3"
			},
			result.ToList()
		);
	}

	[TestMethod]
	public void ParseStatements_CommentsArePreservedInStatement()
	{
		const string input = @"
-- This is a comment
SELECT 1;
-- Another comment
SELECT 2;
		";

		var result = _parser.ParseStatements(input);

		CollectionAssert.AreEqual(
			new List<string>
			{
				$"-- This is a comment{Environment.NewLine}SELECT 1",
				$"-- Another comment{Environment.NewLine}SELECT 2"
			},
			result.ToList()
		);
	}
}
