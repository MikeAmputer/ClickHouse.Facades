using System.Text.RegularExpressions;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

internal class DefaultMigrationFileNameParser : IMigrationFileNameParser
{
	private static readonly Regex Pattern = new(@"^(\d+)_([a-zA-Z0-9_]+)\.(up|down)\.sql$", RegexOptions.IgnoreCase);

	public bool TryParse(string fileName, out MigrationFileInfo? fileInfo)
	{
		fileInfo = null;

		if (fileName.IsNullOrWhiteSpace())
		{
			return false;
		}

		var match = Pattern.Match(fileName);

		if (!match.Success)
		{
			return false;
		}

		if (!ulong.TryParse(match.Groups[1].Value, out var index))
		{
			return false;
		}

		if (!Enum.TryParse<MigrationDirection>(match.Groups[3].Value, ignoreCase: true, out var direction))
		{
			return false;
		}

		fileInfo = new MigrationFileInfo
		{
			Index = index,
			Name = match.Groups[2].Value,
			Direction = direction
		};

		return true;
	}
}
