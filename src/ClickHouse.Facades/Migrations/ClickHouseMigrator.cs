using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

internal class ClickHouseMigrator : IClickHouseMigrator
{
	private readonly IClickHouseContextFactory<ClickHouseMigrationContext> _migrationContextFactory;
	private readonly IClickHouseMigrationsLocator _migrationsLocator;

	private readonly string _migrationsDatabase;

	public ClickHouseMigrator(
		IClickHouseContextFactory<ClickHouseMigrationContext> migrationContextFactory,
		IClickHouseMigrationsLocator migrationsLocator,
		IClickHouseMigrationInstructions instructions)
	{
		_migrationContextFactory = migrationContextFactory
			?? throw new ArgumentNullException(nameof(migrationContextFactory));

		_migrationsLocator = migrationsLocator
			?? throw new ArgumentNullException(nameof(migrationsLocator));

		ExceptionHelpers.ThrowIfNull(instructions);

		_migrationsDatabase = instructions.DatabaseName;

		if (_migrationsDatabase.IsNullOrWhiteSpace())
		{
			throw new ArgumentException($"Migrations database name is null or white space.");
		}
	}

	public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
	{
		await using var context = _migrationContextFactory.CreateContext();

		var facade = context.GetFacade<ClickHouseMigrationFacade>();
		facade.DbName = _migrationsDatabase;

		await EnsureDatabaseCreated(context, cancellationToken);
		await facade.EnsureMigrationsTableCreatedAsync(cancellationToken);

		var migrationsResolver = new MigrationsResolver(
			await facade.GetAppliedMigrationsAsync(cancellationToken),
			_migrationsLocator.GetMigrations());

		foreach (var migration in migrationsResolver.GetMigrationsToApply())
		{
			await facade.ApplyMigrationAsync(migration, cancellationToken);
		}
	}

	private async Task EnsureDatabaseCreated(
		ClickHouseMigrationContext context,
		CancellationToken cancellationToken)
	{
		var database = context.Database;

		context.ChangeDatabase("");

		await context.GetFacade<ClickHouseMigrationFacade>()
			.EnsureDatabaseCreatedAsync(cancellationToken);

		context.ChangeDatabase(database);
	}
}
