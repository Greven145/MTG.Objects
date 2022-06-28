using System;
using FluentAssertions;
using MTG.Objects.ValueObjects;
using Xunit;

namespace MTG.Objects.Tests.Unit.ValueObjects;

public class NumberOfCardsTests
{
    [Fact]
    public void Constructor_Sets_Value()
    {
        //assemble
        var expectedInteger = 4;

        //act
        var numberOfCards = new NumberOfCards(4);

        //assert
        numberOfCards.Should().Match<NumberOfCards>(x => x == expectedInteger);
    }

    [Fact]
    public void Constructor_WhenGivenANegativeNumber_ThrowAnException()
    {
        //assemble
        static NumberOfCards BadConstructorCall()
        {
            return new NumberOfCards(-1);
        }

        //act
        var result = FluentActions.Invoking(BadConstructorCall);

        //assert
        result.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ImplicitOperator_WhenConvertedToInt_ReturnsTheId()
    {
        //assemble
        var expectedInteger = 4;
        var numberOfCards = new NumberOfCards(expectedInteger);

        //act
        int result = numberOfCards;

        //assert
        result.Should().Be(expectedInteger);
    }

    [Fact]
    public void ExplicitOperator_WhenConvertedFromInt_Converts()
    {
        //assemble
        var expectedId = new NumberOfCards(4);

        //act
        var result = (NumberOfCards)4;

        //assert
        result.Should().BeEquivalentTo(expectedId);
    }

    [Fact]
    public void Equals_WhenGivenAnEquivalentId_ShouldReturnTrue()
    {
        //assemble
        var sourceId = new NumberOfCards(4);
        var targetId = new NumberOfCards(4);

        //act
        var actualResult = sourceId.Equals(targetId);

        //assert
        actualResult.Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenGivenAnSameObject_ShouldReturnTrue()
    {
        //assemble
        var sourceId = new NumberOfCards(4);
        var targetId = sourceId;

        //act
        var actualResult = sourceId.Equals(targetId);

        //assert
        actualResult.Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenGivenNull_ShouldReturnFalse()
    {
        //assemble
        var sourceId = new NumberOfCards(4);
        NumberOfCards targetId = null!;

        //act
        var actualResult = sourceId.Equals(targetId);

        //assert
        actualResult.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenGivenADifferentType_ShouldReturnFalse()
    {
        //assemble
        var sourceId = new NumberOfCards(4);
        var targetId = new object();

        //act
        var actualResult = sourceId.Equals(targetId);

        //assert
        actualResult.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WhenGivenAnEquivalentId_ShouldReturnTrue()
    {
        //assemble
        var sourceId = new NumberOfCards(4);
        var targetId = new NumberOfCards(4);

        //act
        var actualResult = sourceId.GetHashCode() == targetId.GetHashCode();

        //assert
        actualResult.Should().BeTrue();
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnStringOfId()
    {
        //assemble
        var expectedNumber = "4";
        var targetNumber = new NumberOfCards(4);

        //act
        var actualResult = targetNumber.ToString();

        //assert
        actualResult.Should().Be(expectedNumber);
    }
}
