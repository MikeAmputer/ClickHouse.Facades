namespace ClickHouse.Facades;

public interface IClickHouseContextFactory<TContext>
	where TContext : ClickHouseContext<TContext>
{
	Task<TContext> CreateContextAsync();

	IClickHouseRetryableExecutor<TContext> CreateRetryableExecutor();
}
