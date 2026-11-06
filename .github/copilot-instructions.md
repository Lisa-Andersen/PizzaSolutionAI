# Copilot instructions

## Project at a glance
This is a small ASP.NET Core Web API that demonstrates pizza menus, ordering, recipes, and stock management, with Swagger UI and PostgreSQL persistence. The repository has two .NET projects and 62 tracked C# source/test files. The app and tests target `net8.0`; there is no `global.json`, `Directory.Build.*`, package lock, linter, or repository setup script. CI selects the .NET 8 SDK. Local validation used .NET SDK 10.0.403/MSBuild 18.9.15 with .NET 8 runtimes installed; this combination successfully built and tested the projects. Prefer a .NET 8 SDK to match CI.

Root: `PizzaSolution.sln`, `README.md` (only a title), `.gitignore`, `.gitattributes`, `.github/`. Main code is in `PizzaPlace/`; unit tests are in `Tests/PizzaPlace.Test/`.

## Architecture and where to work
- `PizzaPlace/Program.cs`: app startup, middleware, Swagger/OpenAPI, and dependency-injection registrations. Production repositories are `RecipeRepository` and `StockRepository`.
- `Controllers/`: API endpoints (`api/menu`, `api/order`, `api/recipe`, `api/restocking`, `api/welcome`). The launch profile uses `http://localhost:9009/`; Swagger is at `/swagger`.
- `Services/`: menu, ordering, recipe, and stock business logic; interfaces sit beside implementations.
- `Repositories/`: repository interfaces, PostgreSQL implementations, and in-memory `Fake*` test implementations. Tests use fakes and Moq; they do not require PostgreSQL.
- `Database/Database.cs`: Npgsql access and startup table setup. `Models/` and `Models/Types/`: DTOs and enums. `Factories/` and `Pizzas/`: oven/pizza preparation. `Extensions/`: shared helpers.
- Tests mirror the main code by area (`Controllers`, `Factories`, `Repositories`, `Services`); MSTest tests use Moq and are in `Tests/PizzaPlace.Test/`.
- Project/package definitions: `PizzaPlace/PizzaPlace.csproj` and `Tests/PizzaPlace.Test/PizzaPlace.Test.csproj`. Launch settings: `PizzaPlace/Properties/launchSettings.json`; app settings are in `PizzaPlace/appsettings*.json`.

Follow nearby C# patterns, nullable reference types, async APIs, and DI. Prefer explicit local types; use PascalCase for public types/members and camelCase for locals/parameters. Add XML documentation for new public APIs. Use `PizzaException` for user-facing domain errors. Keep tests alongside the corresponding test area and use the existing fakes/mocks rather than a live database.

## Build and validation
Run commands from the repository root in this order for a clean, reproducible check:

```powershell
dotnet clean PizzaSolution.sln --configuration Release
dotnet test PizzaSolution.sln --configuration Release
dotnet build PizzaSolution.sln --configuration Release --no-restore
```

This sequence was run successfully from clean Release outputs: 46 tests passed (0 failed/skipped; test step about 13 seconds including restore), and the subsequent Release build succeeded. `dotnet test` restores packages implicitly; no separate bootstrap or install step is needed when restore can reach configured NuGet sources. A standalone build that may need restore is `dotnet build PizzaSolution.sln --configuration Release`.

A plain Debug `dotnet test` was also tried but failed after about 15 seconds because a pre-existing, running `PizzaPlace.exe` held `PizzaPlace/bin/Debug/net8.0/PizzaPlace.exe` open (MSB3027/MSB3021). Stop the app if you started it before rebuilding, or use the clean Release sequence above. The initial compile also reported existing warnings: unused `e` in `PizzaPlace/Repositories/StockRepository.cs`, unused `expectedPizzas` in `GiantRevolvingPizzaOvenTests.cs`, and uninitialized `_stringWriter` in `OrderingServiceTests.cs`. Visual Studio's build tool also reported `ENC0033` while a debugger/hot-reload session was active; direct Release `dotnet build` succeeded. Stop/restart debugging if these IDE-only messages appear. No lint/format command is configured; do not invent one.

To run the API, `dotnet run --project PizzaPlace/PizzaPlace.csproj` uses the `PizzaPlace` launch profile and port 9009. **Do not run it against a non-disposable database:** `Program.cs` calls `Database.SetupTables()` at startup, and that setup drops/recreates tables and seeds data. A local PostgreSQL instance and a deliberately disposable database are required. API startup was not run during onboarding for this reason.

## CI and repository-specific cautions
`.github/workflows/dotnet-desktop.yml` is a Windows Desktop/MSIX template, not a clean fit for this web app. Its `dotnet test` step is the relevant validation; later steps require signing secrets and attempt Windows app packaging although this repository has no packaging project. Those packaging steps were not run and are not validated. There are no other tracked scripts, contributing docs, or CI workflows. Check this workflow before assuming a green CI result implies only the test/build commands above.

Trust these instructions and avoid repository-wide rediscovery. Search or open more files only when a task needs details not covered here or you find these facts incomplete/out of date.
