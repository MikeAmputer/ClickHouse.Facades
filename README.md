# ClickHouse.Facades
[![Latest version](https://img.shields.io/nuget/v/ClickHouse.Facades)](https://www.nuget.org/packages/ClickHouse.Facades/)
[![License](https://img.shields.io/github/license/MikeAmputer/ClickHouse.Facades)](https://github.com/MikeAmputer/ClickHouse.Facades/blob/master/LICENSE)
[![Wiki](https://img.shields.io/badge/wiki-docs-lightgrey?logo=github)](https://github.com/MikeAmputer/ClickHouse.Facades/wiki)

.NET package for managing [ClickHouse](https://github.com/ClickHouse/ClickHouse) migrations and organizing raw SQL through structured database contexts.
Makes it easy to separate concerns across features, services, or tables.
Built on [ClickHouse.Driver](https://github.com/ClickHouse/clickhouse-cs) ([ClickHouse.Client](https://github.com/DarkWanderer/ClickHouse.Client) for version `< 3.0.0`) and tested in production.

[Does OLAP need an ORM?](https://clickhou.se/3Jy2uF8)

> [!NOTE]
> This is an unofficial package and is not affiliated with or endorsed by ClickHouse Inc.
> 
> "ClickHouse" is a registered trademark of ClickHouse Inc. — [clickhouse.com](https://clickhouse.com/)

## Key Features
- **Migrations:** allows you to perform raw SQL migrations on your ClickHouse database.
  - Rollback support
  - Supports file-based migrations
    - Customizable migration file name parser
    - Customizable file content parser
  - Supports code-defined (inline) migrations using C# classes
  - Conditional migrations based on ClickHouse server version
  - [Migration template](https://github.com/MikeAmputer/ClickHouse.Facades/tree/master/src/ClickHouse.Facades.Templates) for .NET CLI
- **Contexts:** provides a way to work with ClickHouse contexts, allowing you to organize your database operations in a structured manner.
  - Context-specific migrations, allowing separate migration management for distinct packages or components
  - Parametrized queries (anonymous type parameters supported)
  - Query cancellation by termination on ClickHouse side
  - Transactions support (keeper is required)
  - Retryable contexts
  - [Dapper](https://github.com/DapperLib/Dapper) compatibility (reader deserialization)
  - Provides all the features of the ClickHouse.Client package
  - Fully async contract
- **Testing toolkit:** provides tools for unit testing components of ClickHouse.Facades with the dedicated [ClickHouse.Facades.Testing](https://github.com/MikeAmputer/ClickHouse.Facades/tree/master/src/ClickHouse.Facades.Testing) package.
  - Facades mocking
  - Specific requests mocking and tracking
- **Comprehensive documentation** presented in [repository Wiki](https://github.com/MikeAmputer/ClickHouse.Facades/wiki).

## Migrations Usage
Implement `IClickHouseMigrationInstructions` and `IClickHouseMigrationsLocator`
([example](https://github.com/MikeAmputer/ClickHouse.Facades/tree/master/examples/Example.Simple/Migrations/Settings))
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
([example](https://github.com/MikeAmputer/ClickHouse.Facades/tree/master/examples/Example.Simple/Context))
and register them as DI services
```csharp
services.AddClickHouseContext<ExampleContext, ExampleContextFactory>(builder => builder
    .AddFacade<UsersFacade>()
    .AddFacade<OrdersFacade>());
```
Request `IClickHouseContextFactory<TContext>` service to create context
```csharp
await using var context = await contextFactory.CreateContextAsync();

var ordersFacade = context.GetFacade<OrdersFacade>();

await ordersFacade.GetOrders();
```
You can create as many contexts as you need with any number of facades. Facades are built via DI and are stateful within context lifetime.

> [!NOTE]
> You can perform migrations on your ClickHouse database without the necessity of implementing contexts.

## Examples

For a deeper understanding and practical use cases of how to implement migrations and facades, please refer to the [examples folder](https://github.com/MikeAmputer/ClickHouse.Facades/tree/master/examples) provided in the repository. 
