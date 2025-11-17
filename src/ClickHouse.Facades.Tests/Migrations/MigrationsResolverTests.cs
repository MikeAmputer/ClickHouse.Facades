using ClickHouse.Facades.Migrations;
using Moq;

namespace ClickHouse.Facades.Tests;

[TestClass]
public class MigrationsResolverTests
{
	[TestMethod]
	public void DuplicateAppliedMigrations_ConstructorThrows()
	{
		var appliedMigration = _1_FirstMigration.AsApplied();
		var duplicateAppliedMigration = _1_FirstMigration.AsApplied();

		Assert.ThrowsExactly<InvalidOperationException>(() => new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { appliedMigration, duplicateAppliedMigration },
			locatedMigrations: new List<ClickHouseMigration>()));
	}

	[TestMethod]
	public void DuplicateLocatedMigrations_ConstructorThrows()
	{
		var migrationMock = _1_FirstMigration.AsMock();
		var duplicateMigrationMock = _1_FirstMigration.AsMock();

		Assert.ThrowsExactly<InvalidOperationException>(() => new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>(),
			locatedMigrations: new List<ClickHouseMigration> { migrationMock.Object, duplicateMigrationMock.Object }));
	}

	[TestMethod]
	public void NoAppliedMigrations_NoLocatedMigrations_NoMigrationsToApply()
	{
		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>(),
			locatedMigrations: new List<ClickHouseMigration>());

		var migrationsToApply = migrationsResolver
			.GetMigrationsToApply()
			.ToList();

		Assert.IsFalse(migrationsToApply.Any());
	}

	[TestMethod]
	public void SingleAppliedMigration_SameLocatedMigration_NoMigrationsToApply()
	{
		var migrationMock = _1_FirstMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _1_FirstMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { migrationMock.Object });

		var migrationsToApply = migrationsResolver
			.GetMigrationsToApply()
			.ToList();

		Assert.IsFalse(migrationsToApply.Any());
	}

	[TestMethod]
	public void NoAppliedMigrations_SingleLocatedMigration_OneMigrationToApply()
	{
		var migrationMock = _1_FirstMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>(),
			locatedMigrations: new List<ClickHouseMigration> { migrationMock.Object });

		var migrationsToApply = migrationsResolver
			.GetMigrationsToApply()
			.ToList();

		Assert.HasCount(1, migrationsToApply);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, migrationsToApply.Single().Index);
	}

	[TestMethod]
	public void NoAppliedMigrations_TwoLocatedMigrations_MigrationsToApplyOrderedByIndex()
	{
		var firstMigrationMock = _1_FirstMigration.AsMock();
		var secondMigrationMock = _2_SecondMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>(),
			locatedMigrations: new List<ClickHouseMigration> { secondMigrationMock.Object, firstMigrationMock.Object });

		var migrationsToApply = migrationsResolver
			.GetMigrationsToApply()
			.ToList();

		Assert.HasCount(2, migrationsToApply);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, migrationsToApply.First().Index);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, migrationsToApply.Last().Index);
	}

	[TestMethod]
	public void FirstMigrationApplied_TwoLocatedMigrations_SecondMigrationToApply()
	{
		var firstMigrationMock = _1_FirstMigration.AsMock();
		var secondMigrationMock = _2_SecondMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _1_FirstMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { firstMigrationMock.Object, secondMigrationMock.Object });

		var migrationsToApply = migrationsResolver
			.GetMigrationsToApply()
			.ToList();

		Assert.HasCount(1, migrationsToApply);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, migrationsToApply.Single().Index);
	}

	[TestMethod]
	public void MigrationApplied_NotLocated_GetMigrationsToApplyThrows()
	{
		var secondMigrationMock = _2_SecondMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _1_FirstMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { secondMigrationMock.Object });

		Assert.ThrowsExactly<InvalidOperationException>(() => migrationsResolver.GetMigrationsToApply());
	}

	[TestMethod]
	public void OldMigrationLocated_NotApplied_GetMigrationsToApplyThrows()
	{
		var firstMigrationMock = _1_FirstMigration.AsMock();
		var secondMigrationMock = _2_SecondMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _2_SecondMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { firstMigrationMock.Object, secondMigrationMock.Object });

		Assert.ThrowsExactly<InvalidOperationException>(() => migrationsResolver.GetMigrationsToApply());
	}

	[TestMethod]
	public void MigrationIsNotApplied_RollbackToIt_Throws()
	{
		var secondMigrationMock = _2_SecondMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _2_SecondMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { secondMigrationMock.Object });

		Assert.ThrowsExactly<InvalidOperationException>(() =>
			migrationsResolver.GetMigrationsToRollback(_1_FirstMigration.MigrationIndex));
	}

	[TestMethod]
	public void ThreeAppliedMigrations_RollbackToFirst_TwoMigrationsOrderedDesc()
	{
		var firstMigrationMock = _1_FirstMigration.AsMock();
		var secondMigrationMock = _2_SecondMigration.AsMock();
		var thirdMigrationMock = _3_ThirdMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>
			{
				_1_FirstMigration.AsApplied(),
				_2_SecondMigration.AsApplied(),
				_3_ThirdMigration.AsApplied()
			},
			locatedMigrations: new List<ClickHouseMigration>
			{
				firstMigrationMock.Object,
				secondMigrationMock.Object,
				thirdMigrationMock.Object
			});

		var toRollback = migrationsResolver
			.GetMigrationsToRollback(_1_FirstMigration.MigrationIndex)
			.ToList();

		Assert.HasCount(2, toRollback);
		Assert.AreEqual(_3_ThirdMigration.MigrationIndex, toRollback.First().Index);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, toRollback.Last().Index);
	}

	[TestMethod]
	public void MigrationNotLocated_RollbackOverIt_Throws()
	{
		var firstMigrationMock = _1_FirstMigration.AsMock();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>
			{
				_1_FirstMigration.AsApplied(),
				_2_SecondMigration.AsApplied()
			},
			locatedMigrations: new List<ClickHouseMigration> { firstMigrationMock.Object });

		Assert.ThrowsExactly<InvalidOperationException>(() =>
			migrationsResolver.GetMigrationsToRollback(_1_FirstMigration.MigrationIndex));
	}
}
