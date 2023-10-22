namespace ClickHouse.Facades.Utility;

internal static class StringExtensions
{
	internal static bool IsNullOrWhiteSpace(this string? value)
	{
		return string.IsNullOrWhiteSpace(value);
	}

	internal static Dictionary<string, string?> GetConnectionStringParameters(
		this string connectionString,
		bool throwOnDuplicate = true)
	{
		var result = new Dictionary<string, string?>();

		var parameters = connectionString
			.Split(';', StringSplitOptions.RemoveEmptyEntries)
			.Select(entry => entry.Trim())
			.Select(param => param
				.Split('=', StringSplitOptions.RemoveEmptyEntries)
				.Select(entry => entry.Trim())
				.ToArray())
			.Select(param => (Key: param[0].ToLower(), Value: param.Length == 1 ? null : param[1]));

		foreach (var param in parameters)
		{
			if (!result.TryAdd(param.Key, param.Value) && throwOnDuplicate)
			{
				throw new InvalidOperationException(
					$"Connection string has duplicate parameter '{param.Key}'.");
			}
		}

		return result;
	}
}
