# MTG.Objects

A collection of objects and utilities for working with Magic: The Gathering services and data. I would frequently have to write classes and objects when writing projects
that worked with Magic cards, and have collected some here for ease of consumption and re-use

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