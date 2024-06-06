using ClickHouse.Facades.Extensions;
using Microsoft.Extensions.Options;

namespace ClickHouse.Facades.Example;

public class ExampleContextFactory : ClickHouseContextFactory<ExampleContext>
{
	private readonly string _connectionString;

	public ExampleContextFactory(IOptions<ClickHouseConfig> config)
	{
		ArgumentNullException.ThrowIfNull(config);

		_connectionString = config.Value.ConnectionString;
	}

	protected override ClickHouseRetryPolicy DefaultRetryPolicy => new()
	{
		RetryCount = 2,
		RetryDelayProvider = retryAttempt => TimeSpan.FromSeconds(1 << retryAttempt),
		TransientExceptionPredicate = ex =>
		{
			// https://github.com/ClickHouse/ClickHouse/blob/master/src/Common/ErrorCodes.cpp
			HashSet<int> transientErrorCodes = [159, 173, 201, 202, 203, 204, 209, 210, 216, 236, 290, 364, 425, 473];

			return transientErrorCodes.Contains(ex.TryGetErrorCode());
		}
	};

	protected override void SetupContextOptions(ClickHouseContextOptionsBuilder<ExampleContext> optionsBuilder)
	{
		optionsBuilder
			.WithConnectionString(_connectionString)
			.ForceSessions()
			.SetupTransactions(options => options
				.AllowMultipleTransactions()
				.AutoRollbackTransaction());
	}
}
