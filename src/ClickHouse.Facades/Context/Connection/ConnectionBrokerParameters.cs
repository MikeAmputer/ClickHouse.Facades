using ClickHouse.Driver;
using ClickHouse.Driver.ADO;

namespace ClickHouse.Facades;

internal class ConnectionBrokerParameters
{
	public ClickHouseClient? Client { get; init; }
	public QueryOptionsBuilder? QueryOptionsBuilder { get; init; }
	public ClickHouseConnection? Connection { get; init; }

	public ICommandExecutionStrategy? CommandExecutionStrategy { get; init; }

	public IClickHouseCommandExecutionListener? CommandExecutionListener { get; init; }
}
