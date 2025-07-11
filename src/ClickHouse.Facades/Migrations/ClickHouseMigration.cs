﻿namespace ClickHouse.Facades.Migrations;

public abstract class ClickHouseMigration
{
	internal virtual ulong Index => MigrationInfo.Index;
	internal virtual string Name => MigrationInfo.Name;

	protected internal abstract void Up(ClickHouseMigrationBuilder migrationBuilder);

	protected internal virtual void Down(ClickHouseMigrationBuilder migrationBuilder)
	{
		throw new NotImplementedException(
			$"Migration '{Index}_{Name}' has no Down() implementation.");
	}

	private ClickHouseMigrationAttribute MigrationInfo => GetType()
		.GetCustomAttributes(typeof(ClickHouseMigrationAttribute), true)
		.Cast<ClickHouseMigrationAttribute>()
		.Single();
}
