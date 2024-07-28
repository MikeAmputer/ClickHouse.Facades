namespace ClickHouse.Facades.Example;

public class ExampleContext : ClickHouseContext<ExampleContext>
{
	public OrdersFacade Orders => GetFacade<OrdersFacade>();
}
