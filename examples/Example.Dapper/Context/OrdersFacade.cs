using Dapper;

namespace ClickHouse.Facades.Example;

public class OrdersFacade : ClickHouseFacade<ExampleContext>
{
	public Task InsertOrder(Order order, CancellationToken cancellationToken = default)
	{
		return ExecuteNonQueryAsync(
			"insert into example_orders format Values " +
			"({UserId:UInt32}, {OrderId:UInt64}, {DateTimeUtc:DateTime64(3, 'UTC')}, {Price:Decimal64(6)})",
			order,
			cancellationToken);
	}

	public async Task<Order[]> GetOrders(CancellationToken cancellationToken = default)
	{
		var reader = await ExecuteReaderAsync("select * from example_orders", cancellationToken);

		return reader.Parse<Order>().ToArray();
	}

	public Task Truncate(CancellationToken cancellationToken = default)
	{
		return ExecuteNonQueryAsync("truncate table example_orders", cancellationToken);
	}
}
