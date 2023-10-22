namespace ClickHouse.Facades.SqlBuilders;

internal static class ClickHouseSqlBuilderExtensions
{
	internal static string ToClickHouseBracedArguments(this string[] args)
	{
		return !args.Any() ? string.Empty : $"({string.Join(", ", args)})";
	}

	internal static string ToClickHouseSettings(this Dictionary<string, string> args)
	{
		if (!args.Any())
		{
			return string.Empty;
		}

		var values = args
			.Select(kv => $"{kv.Key} = {kv.Value}");

		var stringValues = string.Join(", ", values);

		return $"settings {stringValues}";
	}
}
