# ClickHouse.Facades.Testing
Dedicated testing toolkit tailored for unit testing components within the [ClickHouse.Facades](https://github.com/MikeAmputer/ClickHouse.Facades) library.

## Key Features
- **Base Testing Class:** Utilize the ClickHouseFacadesTestsCore class,
  a base class designed specifically for testing ClickHouse facades within MSTest v2 framework.
- **Facilitated Mocking:** Seamlessly mock ClickHouse facades
  or specific requests to the ClickHouse database,
  including operations like `ExecuteNonQuery`, `ExecuteScalar`, and `ExecuteReader`.
- **Request Tracking:** Monitor requests made to the ClickHouse database, utilizing `IClickHouseConnectionTracker` interface.

> [!NOTE]
> This package does not reference any testing frameworks,
but it was designed with a focus on MSTest v2.
Therefore, correct behavior is currently guaranteed only for this framework.

## Getting Started
### Installation
To get started, install the ClickHouse.Facades.Testing package via NuGet Package Manager:
```
nuget install ClickHouse.Facades.Testing
```
### Usage
Create your test classes by inheriting from `ClickHouseFacadesTestsCore`.
```csharp
[TestClass]
public class YourTestClass : ClickHouseFacadesTestsCore
{
    protected override void SetupServiceCollection(IServiceCollection services)
    {
        services.AddClickHouseTestContext<MyContext, MyContextFactory>(builder =>
            builder.AddFacade<IMyFacade, MyFacade>());
    }
}
```
If you use exposed factory type, pass optional parameter `exposedFactoryType` to `AddClickHouseTestContext` method.

Now you have two ways to test your application: mock facade abstraction or mock database requests.
#### Mock facade abstraction
To use this approach you should register your facade with abstraction - `.AddFacade<IMyFacade, MyFacade>()`.
```csharp
[TestMethod]
public async Task My_Test()
{
    Mock<IMyFacade> facadeMock = new();
    facadeMock
        .Setup(m => m.SomeRequest())
        .Callback(SomeMethod);
		
    MockFacadeAbstraction(facadeMock.Object);
}
```
#### Mock database requests
You can only mock `ExecuteNonQuery`, `ExecuteScalar` and `ExecuteReader` (`ExecuteDataTable` can be mocked through mocking `ExecuteReader`).
All calls of these methods are mocked to return default values by default.
You can override return values of selected calls by sql string predicate.
Database requests are tracked with `IClickHouseConnectionTracker` (`GetClickHouseConnectionTracker<TContext>`).
`CreateCommand` and `BulkInsert` are throwing exceptions - use facade mock approach.
```csharp
[TestMethod]
public async Task My_Test()
{
    MockExecuteScalar<MyContext>(
        sql => sql == "select count() from my_table",
        () => 100_000);

    MockExecuteReader<MyContext, MyDataTransferObject>(
        sql => sql == "select * from my_second_table",
        new List<MyDataTransferObject>
        {
            new(1, "One"),
            new(2, "Two"),
        },
        ("id", typeof(ulong), item => item.Id),
        ("name", typeof(string), item => item.Name));

    // Act

    var connectionTracker = GetClickHouseConnectionTracker<MyContext>();

    Assert.AreEqual(2, connectionTracker.RecordsCount);
}
```

> [!NOTE]
> To test your code that uses facades, it is not necessary to inherit from the `ClickHouseFacadesTestsCore` class. It is sufficient to use facades through abstractions, register the context via `services.AddClickHouseTestContext`, and replace the facade abstraction in the service collection with a prepared mock, as done in `ClickHouseFacadesTestsCore.MockFacadeAbstraction`.
