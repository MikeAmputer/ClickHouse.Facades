using ClickHouse.Facades.Migrations;

namespace ClickHouse.Facades.Example;

[ClickHouseMigration(202310240941, "ExampleMigration")]
public class ExampleMigration : ClickHouseMigration
{
	protected override void Up(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddRawSqlStatement(@"
create table if not exists example_orders (
	user_id			UInt32,
	order_id		UInt64,
	price			Decimal64(6)
)
engine = MergeTree
primary key (user_id, order_id)
order by (user_id, order_id)
");

		migrationBuilder.AddRawSqlStatement(@"
create table if not exists example_user_total_expenses (
	user_id			UInt32,
	expenses		Decimal64(6)
)
engine = SummingMergeTree
primary key user_id
order by user_id
");

		migrationBuilder.AddRawSqlStatement(@"
create materialized view if not exists example_user_total_expenses_mv
to example_user_total_expenses
as
select
	user_id,
	price as expenses
from example_orders
");
	}

	protected override void Down(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddRawSqlStatement("drop view if exists example_user_total_expenses_mv");

		migrationBuilder.AddRawSqlStatement("drop table if exists example_user_total_expenses");

		migrationBuilder.AddRawSqlStatement("drop table if exists example_orders");
	}
}
