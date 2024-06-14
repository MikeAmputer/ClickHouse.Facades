using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Example;

[ClickHouseMigration(202406020941, "ExampleMigration")]
public class ExampleMigration : ClickHouseMigration
{
	protected override void Up(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddRawSqlStatement("create database dist_db on cluster example_cluster");

		migrationBuilder.AddRawSqlStatement(@"
create table if not exists dist_db.example_table on cluster example_cluster
(
	value		Int32
)
engine = MergeTree
order by value
");

		migrationBuilder.AddRawSqlStatement(@"
create table if not exists dist_db.example_dist_table on cluster example_cluster
(
	value		Int32
)
ENGINE = Distributed('example_cluster', 'dist_db', 'example_table', rand())
");
	}
}
