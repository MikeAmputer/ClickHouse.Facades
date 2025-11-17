using ClickHouse.Driver.ADO;

namespace ClickHouse.Facades;

public interface IClickHouseCommandExecutionListener
{
	public Task ProcessExecutedCommand(ClickHouseCommand command, CancellationToken cancellationToken = default);
}
