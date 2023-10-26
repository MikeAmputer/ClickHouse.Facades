using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

internal class MigrationsResolver
{
	private readonly IOrderedEnumerable<AppliedMigration> _appliedMigrations;
	private readonly IOrderedEnumerable<ClickHouseMigration> _locatedMigrations;

	public MigrationsResolver(
		IEnumerable<AppliedMigration> appliedMigrations,
		IEnumerable<ClickHouseMigration> locatedMigrations)
	{
		ExceptionHelpers.ThrowIfNull(appliedMigrations);
		ExceptionHelpers.ThrowIfNull(locatedMigrations);

		_appliedMigrations = appliedMigrations.OrderBy(m => m.Id);

		_locatedMigrations = locatedMigrations.OrderBy(m => m.Index);
	}

	public IOrderedEnumerable<ClickHouseMigration> GetMigrationsToApply()
	{
		if (!_appliedMigrations.Any())
		{
			return _locatedMigrations;
		}

		ValidateAppliedMigrations();

		var lastApplied = _appliedMigrations.Max(m => m.Id);

		return _locatedMigrations
			.Where(m => m.Index > lastApplied)
			.OrderBy(m => m.Index);
	}

	public IOrderedEnumerable<ClickHouseMigration> GetMigrationsToRollback(ulong targetMigrationId)
	{
		if (_appliedMigrations.All(m => m.Id != targetMigrationId))
		{
			throw new InvalidOperationException("Unable to find target migration for rollback.");
		}

		var locatedMigrationsDictionary = _locatedMigrations.ToDictionary(m => m.Index);

		var result = new List<ClickHouseMigration>();

		foreach (var appliedMigration in _appliedMigrations.Reverse())
		{
			if (appliedMigration.Id == targetMigrationId)
			{
				break;
			}

			if (locatedMigrationsDictionary.TryGetValue(appliedMigration.Id, out var migration))
			{
				result.Add(migration);
			}
		}

		return result.OrderByDescending(m => m.Index);
	}

	private void ValidateAppliedMigrations()
	{
		for (var i = 0; i < _appliedMigrations.Count(); i++)
		{
			var applied = _appliedMigrations.ElementAt(i);
			var located = _locatedMigrations.ElementAt(i);

			if (applied.Id != located.Index || applied.Name != located.Name)
			{
				throw new InvalidOperationException(
					"Inconsistent schema. Equivalent migrations were expected. " +
					$"Applied: {applied.Id}_{applied.Name}; " +
					$"Located: {located.Index}_{located.Name}.");
			}
		}
	}
}
