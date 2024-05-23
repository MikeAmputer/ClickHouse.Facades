using ClickHouse.Facades.Migrations;

// ReSharper disable InconsistentNaming

namespace $namespace$;

[ClickHouseMigration($index$, "$title$")]
public class $title$_ClickHouseMigration : ClickHouseMigration
{
	protected override void Up(ClickHouseMigrationBuilder migrationBuilder)
	{
		// migrationBuilder.AddRawSqlStatement("create table if not exists ...")
	}

	protected override void Down(ClickHouseMigrationBuilder migrationBuilder)
	{
		// migrationBuilder.AddRawSqlStatement("drop table if exists ...")
	}
}
