namespace ClickHouse.Facades.Migrations;

public class ClickHouseVersion : IComparable<ClickHouseVersion>
{
	public int Major { get; }
	public int Minor { get; }

	public ClickHouseVersion(string version)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			throw new ArgumentException("Database version string is null or empty.", nameof(version));
		}

		var parts = version.Split('.', StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length < 2)
		{
			throw new FormatException("Version must include at least major and minor components, e.g. '24.6'.");
		}

		if (!int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor))
		{
			throw new FormatException("Invalid numeric version format.");
		}

		Major = major;
		Minor = minor;
	}

	public static implicit operator ClickHouseVersion(string version) => new(version);

	public int CompareTo(ClickHouseVersion? other)
	{
		if (other is null)
		{
			return 1;
		}

		var majorComparison = Major.CompareTo(other.Major);
		return majorComparison != 0 ? majorComparison : Minor.CompareTo(other.Minor);
	}

	public static bool operator <(ClickHouseVersion a, ClickHouseVersion b) => a.CompareTo(b) < 0;
	public static bool operator >(ClickHouseVersion a, ClickHouseVersion b) => a.CompareTo(b) > 0;
	public static bool operator <=(ClickHouseVersion a, ClickHouseVersion b) => a.CompareTo(b) <= 0;
	public static bool operator >=(ClickHouseVersion a, ClickHouseVersion b) => a.CompareTo(b) >= 0;
	public static bool operator ==(ClickHouseVersion a, ClickHouseVersion b) => a.CompareTo(b) == 0;
	public static bool operator !=(ClickHouseVersion a, ClickHouseVersion b) => !(a == b);

	public static bool operator <(ClickHouseVersion a, string b) => a < new ClickHouseVersion(b);
	public static bool operator >(ClickHouseVersion a, string b) => a > new ClickHouseVersion(b);
	public static bool operator <=(ClickHouseVersion a, string b) => a <= new ClickHouseVersion(b);
	public static bool operator >=(ClickHouseVersion a, string b) => a >= new ClickHouseVersion(b);
	public static bool operator ==(ClickHouseVersion a, string b) => a == new ClickHouseVersion(b);
	public static bool operator !=(ClickHouseVersion a, string b) => a != new ClickHouseVersion(b);

	public static bool operator <(string a, ClickHouseVersion b) => new ClickHouseVersion(a) < b;
	public static bool operator >(string a, ClickHouseVersion b) => new ClickHouseVersion(a) > b;
	public static bool operator <=(string a, ClickHouseVersion b) => new ClickHouseVersion(a) <= b;
	public static bool operator >=(string a, ClickHouseVersion b) => new ClickHouseVersion(a) >= b;
	public static bool operator ==(string a, ClickHouseVersion b) => new ClickHouseVersion(a) == b;
	public static bool operator !=(string a, ClickHouseVersion b) => new ClickHouseVersion(a) != b;

	public override bool Equals(object? obj) => obj is ClickHouseVersion other && this == other;

	public override int GetHashCode() => HashCode.Combine(Major, Minor);

	public override string ToString() => $"{Major}.{Minor}";
}
