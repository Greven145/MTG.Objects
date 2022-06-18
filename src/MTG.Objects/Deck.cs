using System.Text;
using System.Text.RegularExpressions;
using MTG.Objects.Results;
using MTG.Objects.ValueObjects;

namespace MTG.Objects;

public record Deck(string Name) {
    private const string TxtDeckPattern = @"^(\d+)\s(.*)$";
    private static readonly Regex TextDeckRegex = new(TxtDeckPattern);
    public SubDeck Main { get; } = new();
    public SubDeck Sideboard { get; } = new();

    public static DeckParseResult Parse(string[] strings, string deckName) {
        var deck = new Deck(deckName);
        var errors = new List<string>();

        strings.Aggregate(true, (inMainBoard, line) => ParseLine(line, inMainBoard, errors, deck));
        return new DeckParseResult(deck, errors);
    }

    private static bool ParseLine(string line, bool inMainBoard, ICollection<string> errors, Deck deck) {
        if (IsSideboardSeparator(line)) {
            return false;
        }

        var match = TextDeckRegex.Match(line);
        if (!match.Success) {
            errors.Add("Unable to parse line: " + line);
            return inMainBoard;
        }

        var card = CardFromMatch(match);

        if (inMainBoard) {
            deck.Main.Add(card);
            return inMainBoard;
        }

        deck.Sideboard.Add(card);
        return inMainBoard;
    }

    private static Card CardFromMatch(Match match) {
        var count = int.Parse(match.Groups[1].Value);
        var name = match.Groups[2].Value;
        var card = new Card(name, (NumberOfCards)count);
        return card;
    }

    private static bool IsSideboardSeparator(string line) => line.Length == 0;

    public class SubDeck : List<Card> {
        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine("{");
            foreach (var card in this) {
                sb.AppendLine($"\t{card}");
            }

            sb.Append('}');

            return sb.ToString();
        }
    }
}
