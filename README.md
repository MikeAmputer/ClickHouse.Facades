# ClickHouse.Facades
Raw SQL migrations and contexts for [ClickHouse](https://github.com/ClickHouse/ClickHouse) referencing [ClickHouse.Client](https://github.com/DarkWanderer/ClickHouse.Client)

[![Latest version](https://img.shields.io/nuget/v/ClickHouse.Facades)](https://www.nuget.org/packages/ClickHouse.Facades/)
[![License](https://img.shields.io/github/license/MikeAmputer/ClickHouse.Facades)](https://github.com/MikeAmputer/ClickHouse.Facades/blob/master/LICENSE)

## Key Features
- **Migrations:** allows you to perform raw SQL migrations on your ClickHouse database.
  - Rollback support
  - Fully async contract
- **Contexts:** provides a way to work with ClickHouse contexts, allowing you to organize your database operations in a structured manner.
  - Provides all the features of the ClickHouse.Client package
  - Fully async contract
- **Testing toolkit:** seamlessly integrate unit testing into your ClickHouse.Facades components using the dedicated [ClickHouse.Facades.Testing](https://github.com/MikeAmputer/ClickHouse.Facades/tree/master/src/ClickHouse.Facades.Testing) package. Test ClickHouse contexts and facades effectively, mock facades or specific database requests, and monitor interactions with the ClickHouse database using the provided testing tools.

## Migrations Usage
Implement `IClickHouseMigrationInstructions` and `IClickHouseMigrationsLocator`
([example](https://github.com/MikeAmputer/ClickHouse.Facades/tree/master/src/ClickHouse.Facades.Example/Migrations/Settings))
and register them as DI services
```csharp
services.AddClickHouseMigrations<ClickHouseMigrationInstructions, ClickHouseMigrationsLocator>();
```
You can request `IClickHouseMigrator` service or use `IServiceProviderExtensions` to manage migrations
```csharp
await serviceProvider.ClickHouseMigrateAsync();
```

### Add Migrations
Add `ClickHouseMigration` inheritor with `ClickHouseMigration` attribute
```csharp
[ClickHouseMigration(202310240941, "ExampleMigration")]
public class ExampleMigration : ClickHouseMigration
{
    protected override void Up(ClickHouseMigrationBuilder migrationBuilder)
    {
        // migrationBuilder.AddRawSqlStatement("create table if not exists ...")
    }

    protected override void Down(ClickHouseMigrationBuilder migrationBuilder)
    {
        // migrationBuilder.AddRawSqlStatement("drop table if exists ...")
    }
}
```
The index of `ClickHouseMigrationAttribute` is used to order migrations. It's best to always use idempotent statements (for example with `if [not] exists`) since migration may fail.

## Context Usage
Implement the following class inheritors: `ClickHouseContext<TContext>`, `ClickHouseContextFactory<TContext>`, `ClickHouseFacade<TContext>`
([example](https://github.com/MikeAmputer/ClickHouse.Facades/tree/master/src/ClickHouse.Facades.Example/Context))
and register them as DI services
```csharp
services.AddClickHouseContext<ExampleContext, ExampleContextFactory>(builder => builder
    .AddFacade<UsersFacade>()
    .AddFacade<OrdersFacade>());
```
Request `IClickHouseContextFactory<TContext>` service to create context
```csharp
await using var context = contextFactory.CreateContext();

var ordersFacade = context.GetFacade<OrdersFacade>();

await ordersFacade.GetOrders();
```
You can create as many contexts as you need with any number of facades. Facades are built via DI and are stateful within context lifetime.

> ***Note:*** You can perform migrations on your ClickHouse database without the necessity of implementing contexts.

## Documentation
Documentation will be presented in [repository Wiki](https://github.com/MikeAmputer/ClickHouse.Facades/wiki) (WIP)
