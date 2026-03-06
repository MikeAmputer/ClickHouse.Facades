using System.Data;

namespace ClickHouse.Facades.Extensions;

internal static class DataReaderExtensions
{
	public static string[] GetColumnNames(this IDataReader reader)
	{
		var count = reader.FieldCount;
		var names = new string[count];
		for (var i = 0; i < count; i++)
		{
			names[i] = reader.GetName(i);
		}

		return names;
	}

	internal static IEnumerable<object[]> AsEnumerable(this IDataReader reader)
	{
		while (reader.Read())
		{
			var values = new object[reader.FieldCount];
			reader.GetValues(values);
			yield return values;
		}
	}
}
