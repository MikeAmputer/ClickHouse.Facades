using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Example;

[ClickHouseMigration(202406020941, "ExampleMigration")]
public class ExampleMigration : ClickHouseMigration
{
	protected override void Up(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddRawSqlStatement(@"
create table if not exists example_table
(
	value		Int32
)
engine = MergeTree
order by value
");
	}
}
