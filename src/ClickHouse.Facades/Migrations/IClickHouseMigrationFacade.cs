﻿namespace ClickHouse.Facades.Migrations;

internal interface IClickHouseMigrationFacade
{
	Task EnsureMigrationsTableCreatedAsync(CancellationToken cancellationToken);

	Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken);

	Task<List<AppliedMigration>> GetAppliedMigrationsAsync(CancellationToken cancellationToken);

	Task ApplyMigrationAsync(ClickHouseMigration migration, CancellationToken cancellationToken);

	Task RollbackMigrationAsync(ClickHouseMigration migration, CancellationToken cancellationToken);
}
