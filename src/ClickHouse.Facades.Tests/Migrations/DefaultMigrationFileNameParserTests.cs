using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Tests;

[TestClass]
public class DefaultMigrationFileNameParserTests
{
	private DefaultMigrationFileNameParser _parser = null!;

	[TestInitialize]
	public void Setup()
	{
		_parser = new DefaultMigrationFileNameParser();
	}

	[TestMethod]
	public void TryParse_ValidUpFileName_ReturnsTrueAndParsesCorrectly()
	{
		const string fileName = "00123_init_schema.up.sql";

		var result = _parser.TryParse(fileName, out var fileInfo);

		Assert.IsTrue(result);
		Assert.IsNotNull(fileInfo);
		Assert.AreEqual(123UL, fileInfo.Index);
		Assert.AreEqual("init_schema", fileInfo.Name);
		Assert.AreEqual(MigrationDirection.Up, fileInfo.Direction);
	}

	[TestMethod]
	public void TryParse_ValidDownFileName_ReturnsTrueAndParsesCorrectly()
	{
		const string fileName = "00123_init_schema.down.sql";

		var result = _parser.TryParse(fileName, out var fileInfo);

		Assert.IsTrue(result);
		Assert.IsNotNull(fileInfo);
		Assert.AreEqual(123UL, fileInfo.Index);
		Assert.AreEqual("init_schema", fileInfo.Name);
		Assert.AreEqual(MigrationDirection.Down, fileInfo.Direction);
	}

	[TestMethod]
	public void TryParse_UpperCaseExtensionAndDirection_ReturnsTrue()
	{
		const string fileName = "00123_CreateUsers.UP.SQL";

		var result = _parser.TryParse(fileName, out var fileInfo);

		Assert.IsTrue(result);
		Assert.IsNotNull(fileInfo);
		Assert.AreEqual(123UL, fileInfo.Index);
		Assert.AreEqual("CreateUsers", fileInfo.Name);
		Assert.AreEqual(MigrationDirection.Up, fileInfo.Direction);
	}

	[TestMethod]
	public void TryParse_InvalidExtension_ReturnsFalse()
	{
		const string fileName = "00123_init_schema.up.txt";

		var result = _parser.TryParse(fileName, out var fileInfo);

		Assert.IsFalse(result);
		Assert.IsNull(fileInfo);
	}

	[TestMethod]
	public void TryParse_InvalidIndex_ReturnsFalse()
	{
		const string fileName = "abc_init_schema.up.sql";

		var result = _parser.TryParse(fileName, out var fileInfo);

		Assert.IsFalse(result);
		Assert.IsNull(fileInfo);
	}

	[TestMethod]
	public void TryParse_InvalidDirection_ReturnsFalse()
	{
		const string fileName = "00123_init_schema.sideways.sql";

		var result = _parser.TryParse(fileName, out var fileInfo);

		Assert.IsFalse(result);
		Assert.IsNull(fileInfo);
	}

	[TestMethod]
	public void TryParse_EmptyString_ReturnsFalse()
	{
		var result = _parser.TryParse("", out var fileInfo);

		Assert.IsFalse(result);
		Assert.IsNull(fileInfo);
	}

	[TestMethod]
	public void TryParse_NullString_ReturnsFalse()
	{
		var result = _parser.TryParse(null!, out var fileInfo);

		Assert.IsFalse(result);
		Assert.IsNull(fileInfo);
	}

	[TestMethod]
	public void TryParse_UlongOverflow_ReturnsFalse()
	{
		var fileName = $"{ulong.MaxValue}0_too_big.up.sql";

		var result = _parser.TryParse(fileName, out var fileInfo);

		Assert.IsFalse(result);
		Assert.IsNull(fileInfo);
	}
}
