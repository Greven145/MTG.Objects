using System;
using FluentAssertions;
using MTG.Objects.ValueObjects;
using Xunit;

namespace MTG.Objects.Tests.Unit.ValueObjects;

public class MultiverseIdTests
{
    [Fact]
    public void Constructor_Sets_Value()
    {
        //assemble
        var expectedInteger = 12345;

        //act
        var multiverseId = new MultiverseId(12345);

        //assert
        multiverseId.Should().Match<MultiverseId>(x => x == expectedInteger);
    }

    [Fact]
    public void Constructor_WhenGivenANegativeNumber_ThrowAnException()
    {
        //assemble
        static MultiverseId BadConstructorCall()
        {
            return new MultiverseId(-1);
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
        var expectedInteger = 12345;
        var multiverseId = new MultiverseId(expectedInteger);

        //act
        int result = multiverseId;

        //assert
        result.Should().Be(expectedInteger);
    }

    [Fact]
    public void ExplicitOperator_WhenConvertedFromInt_Converts()
    {
        //assemble
        var expectedId = new MultiverseId(12345);

        //act
        var result = (MultiverseId)12345;

        //assert
        result.Should().BeEquivalentTo(expectedId);
    }

    [Fact]
    public void Equals_WhenGivenAnEquivalentId_ShouldReturnTrue()
    {
        //assemble
        var sourceId = new MultiverseId(12345);
        var targetId = new MultiverseId(12345);

        //act
        var actualResult = sourceId.Equals(targetId);

        //assert
        actualResult.Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenGivenAnSameObject_ShouldReturnTrue()
    {
        //assemble
        var sourceId = new MultiverseId(12345);
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
        var sourceId = new MultiverseId(12345);
        MultiverseId targetId = null!;

        //act
        var actualResult = sourceId.Equals(targetId);

        //assert
        actualResult.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenGivenADifferentType_ShouldReturnFalse()
    {
        //assemble
        var sourceId = new MultiverseId(12345);
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
        var sourceId = new MultiverseId(12345);
        var targetId = new MultiverseId(12345);

        //act
        var actualResult = sourceId.GetHashCode() == targetId.GetHashCode();

        //assert
        actualResult.Should().BeTrue();
    }
}
