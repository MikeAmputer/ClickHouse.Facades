namespace ClickHouse.Facades;

public class ClickHouseRetryPolicy
{
	public Predicate<Exception> TransientExceptionPredicate { get; set; } = _ => true;

	public Action<Exception>? ExceptionHandler { get; set; } = null;

	public int RetryCount { get; set; } = 3;

	public Func<int, TimeSpan> RetryDelayProvider { get; set; } = _ => TimeSpan.Zero;
}
