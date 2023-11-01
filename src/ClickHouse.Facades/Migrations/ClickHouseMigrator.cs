namespace ClickHouse.Facades.Migrations;

internal class ClickHouseMigrator : IClickHouseMigrator
{
	private readonly IClickHouseContextFactory<ClickHouseMigrationContext> _migrationContextFactory;
	private readonly IClickHouseMigrationsLocator _migrationsLocator;

	public ClickHouseMigrator(
		IClickHouseContextFactory<ClickHouseMigrationContext> migrationContextFactory,
		IClickHouseMigrationsLocator migrationsLocator)
	{
		_migrationContextFactory = migrationContextFactory
			?? throw new ArgumentNullException(nameof(migrationContextFactory));

		_migrationsLocator = migrationsLocator
			?? throw new ArgumentNullException(nameof(migrationsLocator));
	}

	public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
	{
		await using var context = _migrationContextFactory.CreateContext();

		var facade = context.MigrationFacade;

		await EnsureDatabaseCreated(context, cancellationToken);
		await facade.EnsureMigrationsTableCreatedAsync(cancellationToken);

		var migrationsResolver = new MigrationsResolver(
			await facade.GetAppliedMigrationsAsync(cancellationToken),
			_migrationsLocator.GetMigrations().ToList());

		foreach (var migration in migrationsResolver.GetMigrationsToApply())
		{
			await facade.ApplyMigrationAsync(migration, cancellationToken);
		}
	}

	public async Task RollbackAsync(ulong targetMigrationId, CancellationToken cancellationToken = default)
	{
		await using var context = _migrationContextFactory.CreateContext();

		var facade = context.MigrationFacade;

		var migrationsResolver = new MigrationsResolver(
			await facade.GetAppliedMigrationsAsync(cancellationToken),
			_migrationsLocator.GetMigrations().ToList());

		foreach (var migration in migrationsResolver.GetMigrationsToRollback(targetMigrationId))
		{
			await facade.RollbackMigrationAsync(migration, cancellationToken);
		}
	}

	private static async Task EnsureDatabaseCreated(
		ClickHouseMigrationContext context,
		CancellationToken cancellationToken)
	{
		var database = context.Database;

		context.ChangeDatabase("");

		await context.MigrationFacade
			.EnsureDatabaseCreatedAsync(cancellationToken);

		context.ChangeDatabase(database);
	}
}
