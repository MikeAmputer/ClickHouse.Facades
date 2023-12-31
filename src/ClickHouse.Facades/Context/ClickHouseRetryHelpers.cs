﻿using ClickHouse.Client;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

public static class ClickHouseRetryHelpers
{
	public static async Task<TResult> ExecuteAsync<TContext, TResult>(
		Func<TContext> contextProvider,
		Func<TContext, Task<TResult>> action,
		Func<int, TimeSpan> retryDelayProvider,
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
				await using var context = contextProvider();

				return await action(context);
			}
			catch (ClickHouseServerException e)
			{
				if (attemptNumber >= retryCount + 1)
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
		Func<TContext> contextProvider,
		Func<TContext, Task> action,
		Func<int, TimeSpan> retryDelayProvider,
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
			exceptionHandler,
			retryCount,
			cancellationToken);
	}

	public static Task<TResult> ExecuteAsync<TContext, TResult>(
		IClickHouseContextFactory<TContext> contextFactory,
		Func<TContext, Task<TResult>> action,
		Func<int, TimeSpan> retryDelayProvider,
		Action<ClickHouseServerException>? exceptionHandler = null,
		int retryCount = 3,
		CancellationToken cancellationToken = default)
		where TContext : ClickHouseContext<TContext>
	{
		return ExecuteAsync(
			contextFactory.CreateContext,
			action,
			retryDelayProvider,
			exceptionHandler,
			retryCount,
			cancellationToken);
	}

	public static Task ExecuteAsync<TContext>(
		IClickHouseContextFactory<TContext> contextFactory,
		Func<TContext, Task> action,
		Func<int, TimeSpan> retryDelayProvider,
		Action<ClickHouseServerException>? exceptionHandler = null,
		int retryCount = 3,
		CancellationToken cancellationToken = default)
		where TContext : ClickHouseContext<TContext>
	{
		return ExecuteAsync(
			contextFactory.CreateContext,
			action,
			retryDelayProvider,
			exceptionHandler,
			retryCount,
			cancellationToken);
	}
}
