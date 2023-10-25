using ClickHouse.Facades;
using ClickHouse.Facades.Example;
using ClickHouse.Facades.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = CreateHostBuilder(args).Build();
var serviceProvider = host.Services;


await serviceProvider.ClickHouseMigrateAsync();


var contextFactory = serviceProvider.GetRequiredService<IClickHouseContextFactory<ExampleContext>>();
await using var context = contextFactory.CreateContext();

await context.Orders.InsertRandomOrders();
var topExpensesUser = await context.Orders.GetTopExpensesUser();

Console.WriteLine(
	$"Top expenses user Id: {topExpensesUser!.UserId}. With total expenses: {topExpensesUser.Expenses:F2}.");


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
			services.AddOptions<OrdersGeneratingConfig>()
				.BindConfiguration(nameof(OrdersGeneratingConfig));

			services.AddClickHouseMigrations<ClickHouseMigrationInstructions, ClickHouseMigrationsLocator>();

			services.AddClickHouseContext<ExampleContext, ExampleContextFactory>(
				builder => builder
					.AddFacade<OrdersFacade>());
		});
