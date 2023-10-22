using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ClickHouseMigrationAttribute : Attribute
{
	internal ulong Index { get; }
	internal string Name { get; }

	public ClickHouseMigrationAttribute(ulong index, string name)
	{
		if (name.IsNullOrWhiteSpace())
		{
			throw new ArgumentException($"{nameof(name)} is null or whitespace.");
		}

		Index = index;
		Name = name;
	}
}
