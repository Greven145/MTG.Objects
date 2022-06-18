using FluentAssertions;
using MTG.Objects.ValueObjects;
using Xunit;

namespace MTG.Objects.Tests.Unit.ValueObjects;

public class EditionTests
{
    [Theory]
    [InlineData("Tempest", "TMP")]
    [InlineData("Core Set 2020", "M20")]
    [InlineData("Adventures in the Forgotten Realms", "AFR")]
    public void TryParse_GivenAValidEditionName_ShouldReturnTheCorrectEdition(string editionName,
        string editionCode)
    {
        //assemble
        var expectedName = editionName.Trim();
        var expectedCode = editionCode.Trim();

        //act
        var (successful, edition) = Edition.TryParse(editionName);

        //assert
        successful.Should().BeTrue();
        edition.Should().NotBeNull();

        var (actualName, actualCode) = edition!;
        actualName.Should().Be(expectedName);
        actualCode.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData("Tempest", "TMP")]
    [InlineData("Core Set 2020", "M20")]
    [InlineData("Adventures in the Forgotten Realms", "AFR")]
    public void TryParse_GivenAValidCodeName_ShouldReturnTheCorrectEdition(string editionName, string editionCode)
    {
        //assemble
        var expectedName = editionName.Trim();
        var expectedCode = editionCode.Trim();

        //act
        var (successful, edition) = Edition.TryParse(editionCode);

        //assert
        successful.Should().BeTrue();
        edition.Should().NotBeNull();

        var (actualName, actualCode) = edition!;
        actualName.Should().Be(expectedName);
        actualCode.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData("Tempest", "TMP")]
    [InlineData("Core Set 2020", "M20")]
    [InlineData("Adventures in the Forgotten Realms", "AFR")]
    [InlineData("      \t Mirrodin", "MRD")]
    [InlineData("      \t Mirrodin", "MRD      ")]
    [InlineData("      \t Mirrodin", "    MRD")]
    [InlineData("Mirrodin      \t ", "MRD")]
    [InlineData("      \t Mirrodin      \t ", "      \t MRD      \t ")]
    public void TryParse_GivenAValidFullName_ShouldReturnTheCorrectEdition(string editionName, string editionCode)
    {
        //assemble
        var expectedName = editionName.Trim();
        var expectedCode = editionCode.Trim();

        //act
        var (successful, edition) = Edition.TryParse($"{editionName} ({editionCode})");

        //assert
        successful.Should().BeTrue();
        edition.Should().NotBeNull();

        var (actualName, actualCode) = edition!;
        actualName.Should().Be(expectedName);
        actualCode.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("The Tempest by William Shakespeare")]
    [InlineData("Core Set 1904")]
    [InlineData("Adventures in the land of Oz")]
    [InlineData("Cons of Tarkir (KTK)")]
    [InlineData("Kaldheim (KLD)")]
    public void TryParse_GivenABadValue_ShouldReturnFalseAndNull(string badText)
    {
        //assemble
        //act
        var (successful, edition) = Edition.TryParse(badText);

        //assert
        successful.Should().BeFalse();
        edition.Should().BeNull();
    }

    [Fact]
    public void ImplicitOperatorToString_ShouldReturnEditionString_InExpectedFormat()
    {
        //assemble
        var edition = Edition.TryParse("Tempest (TMP)").Edition;
        const string expectedResult = "Tempest (TMP)";

        //act
        string actualResult = edition;

        //assert
        actualResult.Should().Be(expectedResult);
    }
}
