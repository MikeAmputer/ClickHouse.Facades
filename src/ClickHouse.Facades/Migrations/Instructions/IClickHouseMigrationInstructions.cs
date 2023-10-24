using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

public interface IClickHouseMigrationInstructions
{
	string GetConnectionString();

	/// <summary>
	/// Database for migrations table. Database will be created if not exists with engine Atomic.
	/// Gets 'database' connection string parameter value as default.
	/// </summary>
	string DatabaseName => GetConnectionString().GetConnectionStringParameters()["database"]
		?? throw new InvalidOperationException("Unable to get 'database' parameter from connection string.");

	bool RetryMigrations => false;

	bool RollbackOnMigrationFail => false;
}
