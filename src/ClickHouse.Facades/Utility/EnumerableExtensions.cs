namespace ClickHouse.Facades.Utility;

internal static class EnumerableExtensions
{
	public static bool HasDuplicates<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> keySelector)
	{
		var uniqueKeys = new HashSet<TKey>();

		return collection.Select(keySelector).Any(key => !uniqueKeys.Add(key));
	}
}
