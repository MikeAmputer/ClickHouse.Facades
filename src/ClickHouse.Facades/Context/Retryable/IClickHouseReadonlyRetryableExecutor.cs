namespace ClickHouse.Facades;

public interface IClickHouseReadonlyRetryableExecutor<out TContext>
	where TContext : ClickHouseContext<TContext>
{
	Task ExecuteAsync(
		Func<TContext, Task> action,
		CancellationToken cancellationToken = default);

	Task<TResult> ExecuteAsync<TResult>(
		Func<TContext, Task<TResult>> action,
		CancellationToken cancellationToken = default);
}
