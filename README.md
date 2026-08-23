# Progressive Overload

This repository contains a .NET 10 prototype for tracking progressive overload data in resistance training.

## Projects

- ProgressiveOverload.Core: reusable library for models, validation, services, and JSON persistence.
- ProgressiveOverload.Console: menu-driven console interface that uses the core library.

## Run

1. Build the solution:
   - `dotnet build ProgressiveOverload.slnx`
2. Run the console app:
   - `dotnet run --project ProgressiveOverload.Console/ProgressiveOverload.Console.csproj`

## Persisted Data Location

The app stores data as JSON in Local AppData:

- Windows path: `%LOCALAPPDATA%\ProgressiveOverload\progressive-overload-data.json`

No database is required in this iteration.
