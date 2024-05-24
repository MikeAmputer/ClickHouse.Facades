# ClickHouse.Facades.Templates
Contains C# class template for [ClickHouse.Facades](https://github.com/MikeAmputer/ClickHouse.Facades) migration creation.

## Installation
```
dotnet new install ClickHouse.Facades.Templates
```

To uninstall the template, use the `uninstall` subcommand:

## Usage
```
dotnet new chmigration --title AddNewTable
```

### Parameters
- **--title**: name of migration. Mandatory parameter.
- **--index**: the index of the migration. Default: current UTC date and time in **"yyyyMMddHHmmss"** format.
- **--dir**: The directory where the migration file will be created. Default: **'Migrations/ClickHouse'**.
- **--namespace**: The namespace for the migration class. Default: project root namespace + **'Migrations.ClickHouse'**.