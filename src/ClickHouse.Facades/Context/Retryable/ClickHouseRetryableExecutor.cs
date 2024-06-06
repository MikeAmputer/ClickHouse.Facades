using ClickHouse.Client;

namespace ClickHouse.Facades;

internal class ClickHouseRetryableExecutor<TContext> : IClickHouseRetryableExecutor<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private readonly IClickHouseContextFactory<TContext> _contextFactory;

	private ClickHouseRetryPolicy _retryPolicy;

	public ClickHouseRetryableExecutor(
		IClickHouseContextFactory<TContext> contextFactory,
		ClickHouseRetryPolicy retryPolicy)
	{
		_contextFactory = contextFactory
			?? throw new ArgumentNullException(nameof(contextFactory));

		_retryPolicy = retryPolicy
			?? throw new ArgumentNullException(nameof(retryPolicy));
	}

	public Task ExecuteAsync(Func<TContext, Task> action, CancellationToken cancellationToken = default)
	{
		return ExecuteAsync(async context =>
			{
				await action(context);

				return 0;
			},
			cancellationToken);
	}

	public async Task<TResult> ExecuteAsync<TResult>(
		Func<TContext, Task<TResult>> action,
		CancellationToken cancellationToken = default)
	{
		var attemptNumber = 1;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				await using var context = await _contextFactory.CreateContextAsync();

				return await action(context);
			}
			catch (ClickHouseServerException e)
			{
				if (attemptNumber >= _retryPolicy.RetryCount + 1)
				{
					throw;
				}

				if (_retryPolicy.TransientExceptionPredicate != null && !_retryPolicy.TransientExceptionPredicate(e))
				{
					throw;
				}

				_retryPolicy.ExceptionHandler?.Invoke(e);
			}

			await Task.Delay(_retryPolicy.RetryDelayProvider(attemptNumber), cancellationToken);
			attemptNumber++;
		}
	}

	public IClickHouseReadonlyRetryableExecutor<TContext> SetRetryPolicy(ClickHouseRetryPolicy retryPolicy)
	{
		_retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));

		return this;
	}

	public IClickHouseReadonlyRetryableExecutor<TContext> UpdateRetryPolicy(Action<ClickHouseRetryPolicy> updateAction)
	{
		updateAction(_retryPolicy);

		return this;
	}
}
