using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Tests;

[TestClass]
public class ClickHouseVersionTests
{
	[TestMethod]
	[DataRow("24.1"), DataRow("24.1.2"), DataRow("24.1.2.3")]
	public void ParseMajorAndMinor(string version)
	{
		var clickHouseVersion = new ClickHouseVersion(version);

		Assert.AreEqual(24, clickHouseVersion.Major);
		Assert.AreEqual(1, clickHouseVersion.Minor);
		Assert.AreEqual("24.1", clickHouseVersion);
	}

	[TestMethod]
	public void CompareMajor()
	{
		var lowerVersion = new ClickHouseVersion("24.1");
		var higherVersion = new ClickHouseVersion("25.1");

		Assert.IsTrue(lowerVersion < higherVersion);
	}

	[TestMethod]
	public void CompareMinor()
	{
		var lowerVersion = new ClickHouseVersion("24.1");
		var higherVersion = new ClickHouseVersion("24.2");

		Assert.IsTrue(lowerVersion < higherVersion);
	}

	[TestMethod]
	public void ParseOnlyMajor_Throws()
	{
		Assert.ThrowsExactly<FormatException>(() => new ClickHouseVersion("24"));
	}
}
