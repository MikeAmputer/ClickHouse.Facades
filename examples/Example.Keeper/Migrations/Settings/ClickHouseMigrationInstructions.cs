using ClickHouse.Facades.Migrations;
using Microsoft.Extensions.Options;

namespace ClickHouse.Facades.Example;

public class ClickHouseMigrationInstructions : IClickHouseMigrationInstructions
{
	private readonly string _connectionString;

	public string ConnectionString => _connectionString;

	public ClickHouseMigrationInstructions(IOptions<ClickHouseConfig> config)
	{
		ArgumentNullException.ThrowIfNull(config);

		_connectionString = config.Value.ConnectionString;
	}
}
