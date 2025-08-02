namespace ClickHouse.Facades.Migrations;

internal class ClickHouseMigrator : IClickHouseMigrator
{
	private readonly IClickHouseContextFactory<ClickHouseMigrationContext> _migrationContextFactory;
	private readonly IClickHouseMigrationsLocator _migrationsLocator;

	private readonly ClickHouseMigrationLog _migrationLog = new();

	public IClickHouseMigrationLog MigrationLog => _migrationLog;

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
		await using var context = await _migrationContextFactory.CreateContextAsync();

		var facade = context.MigrationFacade;

		facade.Log = _migrationLog;

		await EnsureDatabaseCreated(context, cancellationToken);
		await facade.EnsureMigrationsTableCreatedAsync(cancellationToken);

		var migrationsResolver = new MigrationsResolver(
			await GetAppliedMigrations(facade, cancellationToken),
			GetLocatedMigrations());

		TryInitLog(migrationsResolver);

		foreach (var migration in migrationsResolver.GetMigrationsToApply())
		{
			await facade.ApplyMigrationAsync(migration, cancellationToken);

			LogFinalMigration(migration);
		}
	}

	public async Task RollbackAsync(ulong targetMigrationId, CancellationToken cancellationToken = default)
	{
		await using var context = await _migrationContextFactory.CreateContextAsync();

		var facade = context.MigrationFacade;

		facade.Log = _migrationLog;

		var locatedMigrations = GetLocatedMigrations();

		var migrationsResolver = new MigrationsResolver(
			await GetAppliedMigrations(facade, cancellationToken),
			locatedMigrations);

		TryInitLog(migrationsResolver);

		foreach (var migration in migrationsResolver.GetMigrationsToRollback(targetMigrationId))
		{
			LogFinalMigration(migration);

			await facade.RollbackMigrationAsync(migration, cancellationToken);
		}

		_migrationLog.FinalMigrationIndex = targetMigrationId;
		_migrationLog.FinalMigrationName = locatedMigrations
			.SingleOrDefault(m => m.Index == targetMigrationId)
			?.Name;
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

	protected virtual Task<List<AppliedMigration>> GetAppliedMigrations(
		IClickHouseMigrationFacade facade,
		CancellationToken cancellationToken)
	{
		return facade.GetAppliedMigrationsAsync(cancellationToken);
	}

	protected List<ClickHouseMigration> GetLocatedMigrations()
	{
		return _migrationsLocator.GetMigrations().ToList();
	}

	private void TryInitLog(MigrationsResolver migrationsResolver)
	{
		_migrationLog.InitialMigrationIndex ??= migrationsResolver.LastApplied?.Index;
		_migrationLog.InitialMigrationName ??= migrationsResolver.LastApplied?.Name;
		_migrationLog.FinalMigrationIndex ??= migrationsResolver.LastApplied?.Index;
		_migrationLog.FinalMigrationName ??= migrationsResolver.LastApplied?.Name;
	}

	private void LogFinalMigration(ClickHouseMigration migration)
	{
		_migrationLog.FinalMigrationIndex = migration.Index;
		_migrationLog.FinalMigrationName = migration.Name;
	}
}
