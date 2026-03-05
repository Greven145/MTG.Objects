# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MTG.Objects is a C# NuGet library providing common data types for Magic: The Gathering. It uses Roslyn incremental source generators that download data from the MTGJson API at build time and auto-generate SmartEnum classes and set lookup types.

## Build Commands

```bash
# Full build (Tasks project must build first - this is handled automatically by solution dependencies)
dotnet build

# Build only the Tasks project (required before generator can run)
dotnet build src/MTG.Objects.SourceGenerator.Tasks/MTG.Objects.SourceGenerator.Tasks.csproj

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/MTG.Objects.Tests.Unit/MTG.Objects.Tests.Unit.csproj

# Run a specific test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Format code
dotnet format
```

## Architecture

### Three-Project Build Pipeline

The build has a critical ordering dependency:

1. **MTG.Objects.SourceGenerator.Tasks** (`netstandard2.0`) — Custom MSBuild task (`DownloadMtgJsonDataTask`) that downloads `EnumValues.json` and `SetList.json` from `mtgjson.com/api/v5/`. Uses ETag-based HTTP caching with Polly retry policies. Must build before the main library.

2. **MTG.Objects.SourceGenerator** (`netstandard2.0`) — Two Roslyn incremental generators that consume the downloaded JSON files (passed as `AdditionalFiles`):
   - `MtgEnumGenerator` — Reads `EnumValues.json`, generates `SmartEnum` subclasses under `MTG.Objects.Enum.{Category}` (e.g., colors, rarities, frame versions)
   - `MtgSetsGenerator` — Reads `SetList.json`, generates a `Sets` static class with `SetInfo` records under `MTG.Objects.Set`, including dictionary-based lookups by code and name

3. **MTG.Objects** (`netstandard2.1`) — The main library. References the generator as an analyzer. The `.targets` file triggers the download task during `BeforeCompile`. Contains hand-written types: `Card`, `Deck` (with parsing), and value objects (`Edition`, `MultiverseId`, `NumberOfCards`).

### Source Generator Conventions

- Generator projects target `netstandard2.0` with `IsRoslynComponent` and `EnforceExtendedAnalyzerRules`
- Dependencies like `System.Text.Json` and `Humanizer.Core` are bundled as `PrivateAssets="all"` and resolved via a custom `GetDependencyTargetPaths` target
- Marker attributes (`GenerateEnumsAttribute`, `GenerateSetsAttribute`) are emitted by the generators themselves
- Diagnostic IDs: `MTGGEN001`/`MTGGEN002` (enums), `MTGSETS001`/`MTGSETS002` (sets)

### Key Libraries

- **Ardalis.SmartEnum** — Base class for generated enum types
- **Ardalis.GuardClauses** — Argument validation in value objects
- **ValueObject** — Base class for `Edition`, `MultiverseId`, `NumberOfCards`

### Test Projects

- `MTG.Objects.Tests.Unit` (`net10.0`) — xUnit + FluentAssertions for the main library
- `MTG.Objects.SourceGenerator.Tests.Unit` (`net6.0`) — Uses `Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing.XUnit` for generator verification
- `MTG.Object.Generator.Tests.Unit` (`net6.0`) — Legacy generator tests with Moq

## CI/CD

All CI workflows explicitly build the Tasks project before the main solution. The main pipeline (`MTG.Objects-CICD.yml`) builds on Ubuntu, runs tests, packs NuGet, and deploys to NuGet.org on merged PRs to main. Separate workflows run cross-platform tests (Linux/macOS) and SonarQube analysis (Windows).

## Target Frameworks

- Main library: `netstandard2.1`
- Generator and Tasks: `netstandard2.0`
- Unit tests: `net10.0` and `net6.0`
