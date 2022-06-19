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
}
