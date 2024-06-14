namespace ClickHouse.Facades.Example;

public class ExampleContext : ClickHouseContext<ExampleContext>
{
	public TargetFacade TargetFacade => GetFacade<TargetFacade>();

	public DistributedFacade DistributedFacade => GetFacade<DistributedFacade>();
}
