namespace ClickHouse.Facades.Migrations;

internal sealed class ClickHouseMigrationContextFactory : ClickHouseContextFactory<ClickHouseMigrationContext>
{
	private readonly IClickHouseMigrationInstructions _instructions;

	public ClickHouseMigrationContextFactory(IClickHouseMigrationInstructions instructions)
	{
		_instructions = instructions ??
			throw new ArgumentNullException(nameof(instructions));
	}

	protected override void SetupContextOptions(
		ClickHouseContextOptionsBuilder<ClickHouseMigrationContext> optionsBuilder)
	{
		optionsBuilder
			.WithConnectionString(_instructions.GetConnectionString())
			.ForceSessions()
			.AllowDatabaseChanges();

		if (_instructions.HttpClient != null)
		{
			optionsBuilder.WithHttpClient(_instructions.HttpClient);
		}
	}
}
