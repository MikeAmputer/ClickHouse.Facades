using ClickHouse.Facades.Migrations;
using Microsoft.Extensions.Options;

namespace ClickHouse.Facades.Example;

public class ClickHouseMigrationInstructions : IClickHouseMigrationInstructions
{
	private readonly string _connectionString;

	public string ConnectionString => _connectionString;

	public string DatabaseName => "migrations";

	public ClickHouseMigrationInstructions(IOptions<ClickHouseMigrationsConfig> config)
	{
		ArgumentNullException.ThrowIfNull(config);

		_connectionString = config.Value.ConnectionString;
	}
}
