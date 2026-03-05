using FluentAssertions;
using MTG.Objects.ValueObjects;
using Xunit;

namespace MTG.Objects.Tests.Unit;

public class DeckTests
{
    [Fact]
    public void Parse_ValidMainBoardOnly_ReturnsCorrectCards()
    {
        var lines = new[] { "4 Lightning Bolt", "2 Mountain" };

        var result = Deck.Parse(lines, "Test Deck");

        result.Deck.Main.Should().HaveCount(2);
        result.Deck.Sideboard.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MainAndSideboard_SplitsAtEmptyLine()
    {
        var lines = new[] { "4 Lightning Bolt", "", "2 Pyroblast" };

        var result = Deck.Parse(lines, "Test Deck");

        result.Deck.Main.Should().HaveCount(1);
        result.Deck.Sideboard.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_SetsNameCorrectly()
    {
        var lines = new[] { "1 Island" };

        var result = Deck.Parse(lines, "My Deck");

        result.Deck.Name.Should().Be("My Deck");
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsEmptyDeck()
    {
        var lines = System.Array.Empty<string>();

        var result = Deck.Parse(lines, "Empty");

        result.Deck.Main.Should().BeEmpty();
        result.Deck.Sideboard.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("4 Lightning Bolt", 4, "Lightning Bolt")]
    [InlineData("1 Black Lotus", 1, "Black Lotus")]
    [InlineData("20 Forest", 20, "Forest")]
    [InlineData("3 Jace, the Mind Sculptor", 3, "Jace, the Mind Sculptor")]
    public void Parse_ValidCardLine_ParsesQuantityAndName(string line, int expectedCount, string expectedName)
    {
        var result = Deck.Parse(new[] { line }, "Test");

        result.Deck.Main.Should().ContainSingle();
        var card = result.Deck.Main[0];
        ((int)card.Count).Should().Be(expectedCount);
        card.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData("InvalidLine")]
    [InlineData("no number here")]
    [InlineData("abc Lightning Bolt")]
    public void Parse_InvalidLine_AddsToErrors(string line)
    {
        var result = Deck.Parse(new[] { line }, "Test");

        result.Errors.Should().ContainSingle()
            .Which.Should().Contain(line);
        result.Deck.Main.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MixedValidAndInvalid_ParsesValidAndRecordsErrors()
    {
        var lines = new[] { "4 Lightning Bolt", "bad line", "2 Mountain" };

        var result = Deck.Parse(lines, "Test");

        result.Deck.Main.Should().HaveCount(2);
        result.Errors.Should().ContainSingle()
            .Which.Should().Contain("bad line");
    }

    [Fact]
    public void Parse_MultipleEmptyLines_StaysInSideboard()
    {
        var lines = new[] { "4 Lightning Bolt", "", "", "2 Pyroblast" };

        var result = Deck.Parse(lines, "Test");

        result.Deck.Main.Should().HaveCount(1);
        result.Deck.Sideboard.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_EmptyLineFirst_AllCardsGoToSideboard()
    {
        var lines = new[] { "", "2 Pyroblast", "1 Red Blast" };

        var result = Deck.Parse(lines, "Test");

        result.Deck.Main.Should().BeEmpty();
        result.Deck.Sideboard.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_InvalidLineAfterSideboard_MaintainsSideboardContext()
    {
        var lines = new[] { "4 Lightning Bolt", "", "bad line", "2 Pyroblast" };

        var result = Deck.Parse(lines, "Test");

        result.Deck.Main.Should().HaveCount(1);
        result.Deck.Sideboard.Should().HaveCount(1);
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public void SubDeck_ToString_FormatsCardsCorrectly()
    {
        var subDeck = new Deck.SubDeck();
        subDeck.Add(new Card("Lightning Bolt", new NumberOfCards(4)));
        subDeck.Add(new Card("Mountain", new NumberOfCards(2)));

        var output = subDeck.ToString();

        output.Should().Contain("Lightning Bolt");
        output.Should().Contain("Mountain");
        output.Should().StartWith("{");
        output.Should().EndWith("}");
    }

    [Fact]
    public void SubDeck_ToString_EmptyDeck_ReturnsEmptyBraces()
    {
        var subDeck = new Deck.SubDeck();

        var output = subDeck.ToString();

        output.Should().Contain("{");
        output.Should().Contain("}");
    }
}
