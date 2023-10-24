using ClickHouse.Facades;
using ClickHouse.Facades.Example;
using ClickHouse.Facades.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = CreateHostBuilder(args).Build();

var serviceProvider = host.Services;

await serviceProvider.ClickHouseMigrateAsync();


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
		});
