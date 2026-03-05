using FluentAssertions;
using MTG.Objects.ValueObjects;
using Xunit;

namespace MTG.Objects.Tests.Unit;

public class CardTests
{
    [Fact]
    public void Constructor_WhenPassedNames_SetsValues()
    {
        //assemble
        const string actualName = "Black Lotus";
        var actualNumber = new NumberOfCards(4);
        const string expectedName = actualName;
        const int expectedNumber = 4;

        //act
        var card = new Card(actualName, actualNumber);
        var (resultName, resultNumber) = card;

        //assert
        resultName.Should().Be(expectedName);
        resultNumber.Should().Match<NumberOfCards>(x => x == expectedNumber);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var card1 = new Card("Lightning Bolt", new NumberOfCards(4));
        var card2 = new Card("Lightning Bolt", new NumberOfCards(4));

        card1.Should().Be(card2);
    }

    [Fact]
    public void Equality_DifferentName_AreNotEqual()
    {
        var card1 = new Card("Lightning Bolt", new NumberOfCards(4));
        var card2 = new Card("Chain Lightning", new NumberOfCards(4));

        card1.Should().NotBe(card2);
    }

    [Fact]
    public void Equality_DifferentCount_AreNotEqual()
    {
        var card1 = new Card("Lightning Bolt", new NumberOfCards(4));
        var card2 = new Card("Lightning Bolt", new NumberOfCards(3));

        card1.Should().NotBe(card2);
    }
}
