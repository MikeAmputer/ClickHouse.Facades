using ClickHouse.Client;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

public static class ClickHouseRetryHelpers
{
	public static async Task<TResult> ExecuteAsync<TContext, TResult>(
		Func<Task<TContext>> contextProvider,
		Func<TContext, Task<TResult>> action,
		Func<int, TimeSpan> retryDelayProvider,
		Predicate<ClickHouseServerException>? transientExceptionPredicate = null,
		Action<ClickHouseServerException>? exceptionHandler = null,
		int retryCount = 3,
		CancellationToken cancellationToken = default)
		where TContext : ClickHouseContext<TContext>
	{
		ExceptionHelpers.ThrowIfNull(contextProvider);
		ExceptionHelpers.ThrowIfNull(action);
		ExceptionHelpers.ThrowIfNull(retryDelayProvider);

		var attemptNumber = 1;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				await using var context = await contextProvider();

				return await action(context);
			}
			catch (ClickHouseServerException e)
			{
				if (attemptNumber >= retryCount + 1)
				{
					throw;
				}

				if (transientExceptionPredicate != null && !transientExceptionPredicate(e))
				{
					throw;
				}

				exceptionHandler?.Invoke(e);
			}

			await Task.Delay(retryDelayProvider(attemptNumber), cancellationToken);
			attemptNumber++;
		}
	}

	public static Task ExecuteAsync<TContext>(
		Func<Task<TContext>> contextProvider,
		Func<TContext, Task> action,
		Func<int, TimeSpan> retryDelayProvider,
		Predicate<ClickHouseServerException>? transientExceptionPredicate = null,
		Action<ClickHouseServerException>? exceptionHandler = null,
		int retryCount = 3,
		CancellationToken cancellationToken = default)
		where TContext : ClickHouseContext<TContext>
	{
		return ExecuteAsync(
			contextProvider,
			async context =>
			{
				await action(context);

				return 0;
			},
			retryDelayProvider,
			transientExceptionPredicate,
			exceptionHandler,
			retryCount,
			cancellationToken);
	}

	public static Task<TResult> ExecuteAsync<TContext, TResult>(
		IClickHouseContextFactory<TContext> contextFactory,
		Func<TContext, Task<TResult>> action,
		Func<int, TimeSpan> retryDelayProvider,
		Predicate<ClickHouseServerException>? transientExceptionPredicate = null,
		Action<ClickHouseServerException>? exceptionHandler = null,
		int retryCount = 3,
		CancellationToken cancellationToken = default)
		where TContext : ClickHouseContext<TContext>
	{
		return ExecuteAsync(
			contextFactory.CreateContextAsync,
			action,
			retryDelayProvider,
			transientExceptionPredicate,
			exceptionHandler,
			retryCount,
			cancellationToken);
	}

	public static Task ExecuteAsync<TContext>(
		IClickHouseContextFactory<TContext> contextFactory,
		Func<TContext, Task> action,
		Func<int, TimeSpan> retryDelayProvider,
		Predicate<ClickHouseServerException>? transientExceptionPredicate = null,
		Action<ClickHouseServerException>? exceptionHandler = null,
		int retryCount = 3,
		CancellationToken cancellationToken = default)
		where TContext : ClickHouseContext<TContext>
	{
		return ExecuteAsync(
			contextFactory.CreateContextAsync,
			action,
			retryDelayProvider,
			transientExceptionPredicate,
			exceptionHandler,
			retryCount,
			cancellationToken);
	}
}
