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

		Assert.ThrowsException<InvalidOperationException>(() => new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { appliedMigration, duplicateAppliedMigration },
			locatedMigrations: new List<ClickHouseMigration>()));
	}

	[TestMethod]
	public void DuplicateLocatedMigrations_ConstructorThrows()
	{
		Mock<_1_FirstMigration> migrationMock = new();
		Mock<_1_FirstMigration> duplicateMigrationMock = new();

		Assert.ThrowsException<InvalidOperationException>(() => new MigrationsResolver(
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
		Mock<_1_FirstMigration> migrationMock = new();

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
		Mock<_1_FirstMigration> migrationMock = new();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>(),
			locatedMigrations: new List<ClickHouseMigration> { migrationMock.Object });

		var migrationsToApply = migrationsResolver
			.GetMigrationsToApply()
			.ToList();

		Assert.AreEqual(1, migrationsToApply.Count);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, migrationsToApply.Single().Index);
	}

	[TestMethod]
	public void NoAppliedMigrations_TwoLocatedMigrations_MigrationsToApplyOrderedByIndex()
	{
		Mock<_1_FirstMigration> firstMigrationMock = new();
		Mock<_2_SecondMigration> secondMigrationMock = new();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>(),
			locatedMigrations: new List<ClickHouseMigration> { secondMigrationMock.Object, firstMigrationMock.Object });

		var migrationsToApply = migrationsResolver
			.GetMigrationsToApply()
			.ToList();

		Assert.AreEqual(2, migrationsToApply.Count);
		Assert.AreEqual(_1_FirstMigration.MigrationIndex, migrationsToApply.First().Index);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, migrationsToApply.Last().Index);
	}

	[TestMethod]
	public void FirstMigrationApplied_TwoLocatedMigrations_SecondMigrationToApply()
	{
		Mock<_1_FirstMigration> firstMigrationMock = new();
		Mock<_2_SecondMigration> secondMigrationMock = new();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _1_FirstMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { firstMigrationMock.Object, secondMigrationMock.Object });

		var migrationsToApply = migrationsResolver
			.GetMigrationsToApply()
			.ToList();

		Assert.AreEqual(1, migrationsToApply.Count);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, migrationsToApply.Single().Index);
	}

	[TestMethod]
	public void MigrationApplied_NotLocated_GetMigrationsToApplyThrows()
	{
		Mock<_2_SecondMigration> secondMigrationMock = new();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _1_FirstMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { secondMigrationMock.Object });

		Assert.ThrowsException<InvalidOperationException>(() => migrationsResolver.GetMigrationsToApply());
	}

	[TestMethod]
	public void OldMigrationLocated_NotApplied_GetMigrationsToApplyThrows()
	{
		Mock<_1_FirstMigration> firstMigrationMock = new();
		Mock<_2_SecondMigration> secondMigrationMock = new();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _2_SecondMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { firstMigrationMock.Object, secondMigrationMock.Object });

		Assert.ThrowsException<InvalidOperationException>(() => migrationsResolver.GetMigrationsToApply());
	}

	[TestMethod]
	public void MigrationIsNotApplied_RollbackToIt_Throws()
	{
		Mock<_2_SecondMigration> secondMigrationMock = new();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration> { _2_SecondMigration.AsApplied() },
			locatedMigrations: new List<ClickHouseMigration> { secondMigrationMock.Object });

		Assert.ThrowsException<InvalidOperationException>(
			() => migrationsResolver.GetMigrationsToRollback(_1_FirstMigration.MigrationIndex));
	}

	[TestMethod]
	public void ThreeAppliedMigrations_RollbackToFirst_TwoMigrationsOrderedDesc()
	{
		Mock<_1_FirstMigration> firstMigrationMock = new();
		Mock<_2_SecondMigration> secondMigrationMock = new();
		Mock<_3_ThirdMigration> thirdMigrationMock = new();

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

		Assert.AreEqual(2, toRollback.Count);
		Assert.AreEqual(_3_ThirdMigration.MigrationIndex, toRollback.First().Index);
		Assert.AreEqual(_2_SecondMigration.MigrationIndex, toRollback.Last().Index);
	}

	[TestMethod]
	public void MigrationNotLocated_RollbackOverIt_Throws()
	{
		Mock<_1_FirstMigration> firstMigrationMock = new();

		var migrationsResolver = new MigrationsResolver(
			appliedMigrations: new List<AppliedMigration>
			{
				_1_FirstMigration.AsApplied(),
				_2_SecondMigration.AsApplied()
			},
			locatedMigrations: new List<ClickHouseMigration> { firstMigrationMock.Object });

		Assert.ThrowsException<InvalidOperationException>(
			() => migrationsResolver.GetMigrationsToRollback(_1_FirstMigration.MigrationIndex));
	}
}
