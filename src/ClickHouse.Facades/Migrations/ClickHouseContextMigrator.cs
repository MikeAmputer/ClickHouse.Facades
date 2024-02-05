namespace ClickHouse.Facades.Migrations;

internal class ClickHouseContextMigrator<TContext> : ClickHouseMigrator, IClickHouseMigrator<TContext>
	where TContext : ClickHouseContext<TContext>
{
	// ReSharper disable once SuggestBaseTypeForParameterInConstructor
	public ClickHouseContextMigrator(
		IClickHouseContextFactory<ClickHouseMigrationContext> migrationContextFactory,
		IClickHouseMigrationsLocator<TContext> migrationsLocator)
		: base(migrationContextFactory, migrationsLocator)
	{
	}

	protected override async Task<List<AppliedMigration>> GetAppliedMigrations(
		IClickHouseMigrationFacade facade,
		CancellationToken cancellationToken)
	{
		var allAppliedMigrations = await base.GetAppliedMigrations(facade, cancellationToken);

		var locatedMigrations = GetLocatedMigrations()
			.Select(m => (m.Index, m.Name))
			.ToHashSet();

		return allAppliedMigrations
			.Where(appliedMigration => locatedMigrations
				.Contains((appliedMigration.Id, appliedMigration.Name)))
			.ToList();
	}
}
