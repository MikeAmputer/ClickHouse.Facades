namespace ClickHouse.Facades;

public interface IClickHouseRetryableExecutor<out TContext> : IClickHouseReadonlyRetryableExecutor<TContext>
	where TContext : ClickHouseContext<TContext>
{
	IClickHouseReadonlyRetryableExecutor<TContext> SetRetryPolicy(ClickHouseRetryPolicy retryPolicy);

	IClickHouseReadonlyRetryableExecutor<TContext> UpdateRetryPolicy(Action<ClickHouseRetryPolicy> updateAction);
}
