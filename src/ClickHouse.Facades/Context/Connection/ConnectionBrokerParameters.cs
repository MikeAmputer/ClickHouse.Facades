using ClickHouse.Client.ADO;

namespace ClickHouse.Facades;

internal class ConnectionBrokerParameters
{
	public ClickHouseConnection? Connection { get; set; }

	public ICommandExecutionStrategy? CommandExecutionStrategy { get; set; }

	public IClickHouseCommandExecutionListener? CommandExecutionListener { get; set; }
}
