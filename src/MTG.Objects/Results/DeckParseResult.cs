namespace MTG.Objects.Results;

public record DeckParseResult(Deck Deck, List<string> Errors);