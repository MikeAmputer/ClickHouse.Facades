namespace ClickHouse.Facades.Example;

public class ExampleContext : ClickHouseContext<ExampleContext>
{
	public ExampleFacade ExampleFacade => GetFacade<ExampleFacade>();
}
