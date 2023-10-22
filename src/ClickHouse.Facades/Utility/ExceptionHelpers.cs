namespace ClickHouse.Facades.Utility;

internal static class ExceptionHelpers
{
	internal static void ThrowIfNull(object? argument, string? paramName = null)
	{
		if (argument == null)
		{
			throw new ArgumentNullException(paramName);
		}
	}
}
