using ClickHouse.Facades;
using ClickHouse.Facades.Example;
using ClickHouse.Facades.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var host = CreateHostBuilder(args).Build();
var serviceProvider = host.Services;


await serviceProvider.ClickHouseMigrateAsync();


var contextFactory = serviceProvider.GetRequiredService<IClickHouseContextFactory<ExampleContext>>();
await using var context = await contextFactory.CreateContextAsync();

var ordersFacade = context.Orders;

await ordersFacade.Truncate();

await ordersFacade.InsertOrder(new Order
{
	UserId = 10,
	OrderId = 1100,
	DateTimeUtc = DateTime.UtcNow,
	Price = 42.421337M,
});

var orders = await ordersFacade.GetOrders();

foreach (var order in orders)
{
	Console.WriteLine($"{order.OrderId}: {order.UserId} | {order.DateTimeUtc} | {order.Price}");
}


static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureAppConfiguration((context, builder) =>
		{
			var path = Path.Combine(context.HostingEnvironment.ContentRootPath, "appsettings.json");
			builder.AddJsonFile(path, false, true);
		})
		.ConfigureServices((_, services) =>
		{
			services.AddOptions<ClickHouseConfig>()
				.BindConfiguration(nameof(ClickHouseConfig));

			services.AddClickHouseMigrations<ClickHouseMigrationInstructions, ClickHouseMigrationsLocator>();

			services.AddClickHouseContext<ExampleContext, ExampleContextFactory>(
				builder => builder
					.AddFacade<OrdersFacade>());
		});
