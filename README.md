# MTG.Objects

A collection of objects and utilities for working with Magic: The Gathering services and data. I would frequently have to write classes and objects when writing projects
that worked with Magic cards, and have collected some here for ease of consumption and re-use

[![MTG.Objects CI/CD](https://github.com/Greven145/MTG.Objects/actions/workflows/MTG.Objects-CICD.yml/badge.svg)](https://github.com/Greven145/MTG.Objects/actions/workflows/MTG.Objects-CICD.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Greven145_MTG.Objects&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Greven145_MTG.Objects)


## How to install

Instructions can be found on the [NuGet page](TBD) for this repository.

## Generator project

The generator project will download data from MTGJson and create classes that can be used like enums for all the various MTG values.

## Samples

### Deck Parsing

```csharp
    var deckName = "Tron";
    var fileName = $"{deckName}.txt";
    var filePath = "./path/to/deck/files";

    var fileContents = await File.ReadAllLinesAsync(Path.Combine(filePath, fileName));
    var (deck, errors) = Deck.Parse(fileContents, name);
```

### Value Objects

#### Edition

You can use the `Edition.TryParse()` static method to parse a set name or code into a complete object

```csharp
    var (successful, value) = Edition.TryParse("Kaldheim");
	if ( successful ){
		Console.WriteLine(value);
    }
```

```text
Kaldheim (KHM)
```

```csharp
    var (successful, value) = Edition.TryParse("M11");
	if ( successful ){
		Console.WriteLine(value);
    }
```

```text
Magic 2011 (M11)
```

#### MultiverseId

You can use the `MultiverseId.TryParse()` static method to parse an Id into a valid format

```csharp
    var id = (MultiverseId)12345;
	Console.WriteLine(id.ToString());
```

```text
12345
```

```csharp
    var id = (MultiverseId)(-321);
```

```text
System.ArgumentException: Required input id cannot be negative. (Parameter 'id')
```