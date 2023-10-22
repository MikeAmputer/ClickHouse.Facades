namespace ClickHouse.Facades;

public interface IClickHouseContextFactory<out TContext>
	where TContext : ClickHouseContext<TContext>
{
	TContext CreateContext();
}
