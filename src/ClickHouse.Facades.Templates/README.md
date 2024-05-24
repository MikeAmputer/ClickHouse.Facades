# ClickHouse.Facades.Templates
Contains C# class template for [ClickHouse.Facades](https://github.com/MikeAmputer/ClickHouse.Facades) migration creation.

## Installation
```
dotnet new install ClickHouse.Facades.Templates
```

Use `uninstall` subcommand instead of `install` to uninstall.

## Usage
```
dotnet new chmigration --title AddNewTable
```

### Parameters
- **--title**: name of migration. Mandatory parameter.
- **--index**: the index of the migration. Default: current UTC date and time in "yyyyMMddHHmmss" format.
- **--dir**: The directory where the migration file will be created. Default: Migrations/ClickHouse.
- **--namespace**: The namespace for the migration class. Default: project root namespace + Migrations.ClickHouse.