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
var facade = context.ExampleFacade;

await facade.Truncate();
var values = await facade.GetValues();
Console.WriteLine(
	$"Values count before transaction: {values.Length}");

await context.BeginTransactionAsync();

await facade.InsertValue(42);
values = await facade.GetValues();
Console.WriteLine(
	$"Values count inside transaction: {values.Length}");

await context.RollbackTransactionAsync();

values = await facade.GetValues();
Console.WriteLine(
	$"Values count after rollback: {values.Length}");

await context.BeginTransactionAsync();

await facade.InsertValue(42);

await context.CommitTransactionAsync();

values = await facade.GetValues();
Console.WriteLine(
	$"Values count after commit: {values.Length}");


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
					.AddFacade<ExampleFacade>());
		});
