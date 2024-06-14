using ClickHouse.Facades;
using ClickHouse.Facades.Example;
using ClickHouse.Facades.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = CreateHostBuilder(args).Build();
var serviceProvider = host.Services;


await serviceProvider.ClickHouseMigrateAsync();


var contextFactory = serviceProvider.GetRequiredService<ExampleContextFactory>();

await using (var context = await contextFactory.CreateContextAsync())
{
	await context.TargetFacade.Truncate();
}

contextFactory.NextShard();

await using (var context = await contextFactory.CreateContextAsync())
{
	await context.TargetFacade.Truncate();
}

contextFactory.NextShard();

await using (var context = await contextFactory.CreateContextAsync())
{
	await context.TargetFacade.InsertValue(42);
	Console.WriteLine("Node 1 value inserted");

	var values = await context.TargetFacade.GetValues();
	Console.WriteLine(
		$"Node 1 values count: {values.Length}");

	var distValues = await context.DistributedFacade.GetValues();
	Console.WriteLine(
		$"Distributed values count: {distValues.Length}");
}

contextFactory.NextShard();

await using (var context = await contextFactory.CreateContextAsync())
{
	await context.TargetFacade.InsertValue(41);
	Console.WriteLine("\nNode 2 value inserted");

	var values = await context.TargetFacade.GetValues();
	Console.WriteLine(
		$"Node 2 values count: {values.Length}");

	var distValues = await context.DistributedFacade.GetValues();
	Console.WriteLine(
		$"Distributed values count: {distValues.Length}");
}

contextFactory.NextShard();

await using (var context = await contextFactory.CreateContextAsync())
{
	await context.DistributedFacade.InsertValues(100);
	Console.WriteLine("\n100 distributed values inserted.");

	// you can't immediately select values inserted in distributed table
	await Task.Delay(TimeSpan.FromMilliseconds(100));

	var distValues = await context.DistributedFacade.GetValues();
	Console.WriteLine(
		$"Distributed values count (queried on node 1): {distValues.Length}");

	var values = await context.TargetFacade.GetValues();
	Console.WriteLine(
		$"Node 1 values count: {values.Length}");
}

contextFactory.NextShard();

await using (var context = await contextFactory.CreateContextAsync())
{
	var distValues = await context.DistributedFacade.GetValues();
	Console.WriteLine(
		$"Distributed values count (queried on node 2): {distValues.Length}");

	var values = await context.TargetFacade.GetValues();
	Console.WriteLine(
		$"Node 2 values count: {values.Length}");
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
			services.AddOptions<ClickHouseMainConfig>()
				.BindConfiguration(nameof(ClickHouseMainConfig));
			services.AddOptions<ClickHouseShardConfig>()
				.BindConfiguration(nameof(ClickHouseShardConfig));
			services.AddOptions<ClickHouseMigrationsConfig>()
				.BindConfiguration(nameof(ClickHouseMigrationsConfig));

			services.AddClickHouseMigrations<ClickHouseMigrationInstructions, ClickHouseMigrationsLocator>();

			services.AddClickHouseContext<ExampleContext, ExampleContextFactory>(
				builder => builder
					.AddFacade<TargetFacade>()
					.AddFacade<DistributedFacade>(),
				factoryLifetime: ServiceLifetime.Transient,
				exposeFactoryType: true);
		});
