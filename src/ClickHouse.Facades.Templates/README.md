# ClickHouse.Facades.Templates
Contains C# class template for [ClickHouse.Facades](https://github.com/MikeAmputer/ClickHouse.Facades) migration creation.

## Installation
```
dotnet new install ClickHouse.Facades.Templates
```

To update the template, use the `update` subcommand.
To uninstall the template, use the `uninstall` subcommand.
([Microsoft 'dotnet new' docs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new))

## Usage
```
dotnet new chmigration --title AddNewTable
```

### Parameters
- **--title**: name of migration. **Mandatory** parameter.
- **--index**: the index of the migration. **Default**: current UTC date and time in **"yyyyMMddHHmmss"** format.
- **--dir**: The directory where the migration file will be created. **Default**: **'Migrations/ClickHouse'**.
- **--namespace**: The namespace for the migration class. **Default**: project root namespace + **'Migrations.ClickHouse'**.

### Example
If you run the command with default parameters and 'AddNewTable' title, you will get the following file structure:
```
ClickHouse.Facades.Example
├── ...
└── Migrations
    └── ClickHouse
        ├── ...
        └── 20240524093800_AddNewTable.cs
```
and the following C# class:
```c#
using ClickHouse.Facades.Migrations;

// ReSharper disable InconsistentNaming

namespace ClickHouse.Facades.Example.Migrations.ClickHouse;

[ClickHouseMigration(20240524093800, "AddNewTable")]
public class AddNewTable_ClickHouseMigration : ClickHouseMigration
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
