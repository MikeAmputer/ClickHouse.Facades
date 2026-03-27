using ClickHouse.Driver.ADO;

namespace ClickHouse.Facades;

public interface IClickHouseCommandExecutionListener
{
	Task ProcessExecutedCommand(ClickHouseCommand command, CancellationToken cancellationToken = default);
}
