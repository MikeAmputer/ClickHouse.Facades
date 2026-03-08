using System.Reflection;
using System.Text;
using ClickHouse.Driver.ADO;

namespace ClickHouse.Facades.Testing;

internal static class ClickHouseRawResultFactory
{
	private static readonly ConstructorInfo Ctor =
		typeof(ClickHouseRawResult).GetConstructor(
			BindingFlags.NonPublic | BindingFlags.Instance,
			binder: null,
			types: [typeof(HttpResponseMessage)],
			modifiers: null
		)!;

	public static ClickHouseRawResult FromString(string content)
	{
		var response = new HttpResponseMessage
		{
			Content = new StringContent(content, Encoding.UTF8)
		};

		return (ClickHouseRawResult)Ctor.Invoke([response]);
	}

	public static ClickHouseRawResult FromBytes(byte[] bytes)
	{
		var response = new HttpResponseMessage
		{
			Content = new ByteArrayContent(bytes)
		};

		return (ClickHouseRawResult)Ctor.Invoke([response]);
	}

	public static ClickHouseRawResult FromStream(Stream stream)
	{
		var response = new HttpResponseMessage
		{
			Content = new StreamContent(stream)
		};

		return (ClickHouseRawResult)Ctor.Invoke([response]);
	}
}
