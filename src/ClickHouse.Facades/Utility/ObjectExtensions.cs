using System.Reflection;

namespace ClickHouse.Facades.Utility;

internal static class ObjectExtensions
{
	public static Dictionary<string, object?> DeconstructToDictionary(this object obj)
	{
		var dictionary = new Dictionary<string, object?>();

		var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (var prop in properties)
		{
			var value = prop.GetValue(obj);
			dictionary.Add(prop.Name, value);
		}

		return dictionary;
	}
}
