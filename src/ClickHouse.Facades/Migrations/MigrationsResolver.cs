using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

internal class MigrationsResolver
{
	private readonly IOrderedEnumerable<AppliedMigration> _appliedMigrations;
	private readonly IOrderedEnumerable<ClickHouseMigration> _locatedMigrations;

	public AppliedMigration? LastApplied { get; }

	public MigrationsResolver(
		IReadOnlyCollection<AppliedMigration> appliedMigrations,
		IReadOnlyCollection<ClickHouseMigration> locatedMigrations)
	{
		ArgumentNullException.ThrowIfNull(appliedMigrations);
		ArgumentNullException.ThrowIfNull(locatedMigrations);

		if (appliedMigrations.HasDuplicates(m => m.Index))
		{
			throw new InvalidOperationException("Applied migrations collection contains duplicates.");
		}

		if (locatedMigrations.HasDuplicates(m => m.Index))
		{
			throw new InvalidOperationException("Located migrations collection contains duplicates.");
		}

		_appliedMigrations = appliedMigrations.OrderBy(m => m.Index);

		_locatedMigrations = locatedMigrations.OrderBy(m => m.Index);

		LastApplied = _appliedMigrations.LastOrDefault();
	}

	public IOrderedEnumerable<ClickHouseMigration> GetMigrationsToApply()
	{
		if (!_appliedMigrations.Any())
		{
			return _locatedMigrations;
		}

		ValidateAppliedMigrations();

		return _locatedMigrations
			.Where(m => m.Index > (LastApplied?.Index ?? 0))
			.OrderBy(m => m.Index);
	}

	public IOrderedEnumerable<ClickHouseMigration> GetMigrationsToRollback(ulong targetMigrationId)
	{
		if (_appliedMigrations.All(m => m.Index != targetMigrationId))
		{
			throw new InvalidOperationException("Unable to find target migration for rollback.");
		}

		var locatedMigrationsDictionary = _locatedMigrations.ToDictionary(m => m.Index);

		var result = new List<ClickHouseMigration>();

		foreach (var appliedMigration in _appliedMigrations.Reverse())
		{
			if (appliedMigration.Index == targetMigrationId)
			{
				break;
			}

			if (locatedMigrationsDictionary.TryGetValue(appliedMigration.Index, out var migration))
			{
				result.Add(migration);
			}
			else
			{
				throw new InvalidOperationException(
					$"Unable to find located migration '{appliedMigration.Name}'");
			}
		}

		return result.OrderByDescending(m => m.Index);
	}

	private void ValidateAppliedMigrations()
	{
		var appliedList = _appliedMigrations.ToList();
		var locatedList = _locatedMigrations.ToList();

		if (appliedList.Count > locatedList.Count)
		{
			throw new InvalidOperationException(
				"Inconsistent schema. Database contains more applied migrations than located. " +
				$"Applied: {appliedList.Count}; Located: {locatedList.Count}.");
		}

		for (var i = 0; i < appliedList.Count; i++)
		{
			var applied = appliedList[i];
			var located = locatedList[i];

			if (applied.Index != located.Index || applied.Name != located.Name)
			{
				throw new InvalidOperationException(
					"Inconsistent schema. Equivalent migrations were expected. " +
					$"Applied: {applied.Index}_{applied.Name}; " +
					$"Located: {located.Index}_{located.Name}.");
			}
		}
	}
}
