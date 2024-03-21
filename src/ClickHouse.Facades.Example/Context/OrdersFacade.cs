using Microsoft.Extensions.Options;

namespace ClickHouse.Facades.Example;

public class OrdersFacade : ClickHouseFacade<ExampleContext>
{
	private readonly uint _rowsPerRequest;

	public OrdersFacade(IOptions<OrdersGeneratingConfig> config)
	{
		ArgumentNullException.ThrowIfNull(config);

		_rowsPerRequest = config.Value.RowsPerRequest;
	}

	#region InsertRandomOrders
	public async Task InsertRandomOrders(CancellationToken cancellationToken = default)
	{
		var sql = string.Format(InsertRandomOrdersSql, _rowsPerRequest);

		await ExecuteNonQueryAsync(sql, cancellationToken);
	}

	private const string InsertRandomOrdersSql = @"
insert into example_orders

select
	rand() % 10 + 1 as user_id,
	0 as order_id,
	toDecimal64(randUniform(100, 1000), 2) as price
from
	numbers({0})
";
	#endregion

	#region GetTopExpensesUser
	public async Task<UserExpenses?> GetTopExpensesUser(CancellationToken cancellationToken = default)
	{
		var results = await ExecuteQueryAsync(
				GetTopExpensesUserSql,
				UserExpenses.FromReader,
				cancellationToken)
			.ToListAsync(cancellationToken);

		return results.SingleOrDefault();
	}

	private const string GetTopExpensesUserSql = @"
select
	user_id,
	expenses
from
	example_user_total_expenses final
order by
	expenses desc,
	user_id asc
limit 1
";
	#endregion

	#region BulkInsert
	public async Task InsertOrdersBulk(CancellationToken cancellationToken = default)
	{
		await BulkInsertAsync(
			"example_orders",
			Enumerable.Range(0, 100).Select(i => new object[] { i % 10 + 1, i * 2 + 1, (i + 1) / 0.33 }),
			new[] { "user_id", "order_id", "price" },
			batchSize: 45,
			maxDegreeOfParallelism: 2,
			cancellationToken: cancellationToken);
	}

	public async Task CopyOrdersBulk(CancellationToken cancellationToken = default)
	{
		var reader = await ExecuteReaderAsync("select * from example_orders where user_id = 1", cancellationToken);

		await BulkInsertAsync(
			"example_orders",
			reader,
			batchSize: 45,
			maxDegreeOfParallelism: 2,
			cancellationToken: cancellationToken);
	}

	public async Task CopyOrdersDataTable(CancellationToken cancellationToken = default)
	{
		var dataTable = ExecuteDataTable("select * from example_orders where user_id = 10", cancellationToken);

		await BulkInsertAsync(
			"example_orders",
			dataTable,
			batchSize: 45,
			maxDegreeOfParallelism: 2,
			cancellationToken: cancellationToken);
	}
	#endregion

	#region Parametrized
	public async Task InsertOrder(
		int userId,
		int orderId,
		decimal price,
		CancellationToken cancellationToken = default)
	{
		await ExecuteNonQueryAsync(
			InsertOrderSql,
			new {userId, orderId, price},
			cancellationToken);
	}

	private const string InsertOrderSql = @"
insert into example_orders

select
	{userId:UInt32} as user_id,
	{orderId:UInt64} as order_id,
	{price:Decimal64(6)} as price
";
	#endregion
}
