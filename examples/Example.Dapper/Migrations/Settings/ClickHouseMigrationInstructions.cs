using ClickHouse.Facades.Migrations;
using Microsoft.Extensions.Options;

namespace ClickHouse.Facades.Example;

public class ClickHouseMigrationInstructions : IClickHouseMigrationInstructions
{
	public string ConnectionString { get; }

	public ClickHouseMigrationInstructions(IOptions<ClickHouseConfig> config)
	{
		ArgumentNullException.ThrowIfNull(config);

		ConnectionString = config.Value.ConnectionString;
	}
}
