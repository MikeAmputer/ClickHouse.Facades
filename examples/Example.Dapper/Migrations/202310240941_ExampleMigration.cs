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
	date_time_utc	DateTime64(3, 'UTC'),
	price			Decimal64(6)
)
engine = MergeTree
primary key (user_id, order_id)
order by (user_id, order_id)
");
	}

	protected override void Down(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddRawSqlStatement("drop table if exists example_orders");
	}
}
