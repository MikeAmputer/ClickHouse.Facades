using System.Text.RegularExpressions;
using ClickHouse.Driver;

namespace ClickHouse.Facades.Extensions;

public static class ClickHouseServerExceptionExtensions
{
	private const string ErrorCodePattern = @"^Code:\s(\d+).";

	public static int TryGetErrorCode(this ClickHouseServerException exception)
	{
		var match = Regex.Match(exception.Message, ErrorCodePattern);

		if (match.Success)
		{
			var codeString = match.Groups[1].Value;
			var codeParsed = int.TryParse(codeString, out var code);

			if (codeParsed)
			{
				return code;
			}
		}

		return -1;
	}
}
