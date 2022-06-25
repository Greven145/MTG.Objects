using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MTG.Object.Generator.Modules.SetGenerator.Services;
using MTG.Object.Generator.Modules.Shared.Interfaces;

namespace MTG.Object.Generator.Tests.Unit.Modules.SetGenerator.Services; 

public class SetClassWriterTests {
    private const string SolutionFolderName = "TestSolution";
    private readonly IDirectoryService _directoryService;

    private readonly IFileService _fileService;
    private readonly ILogger<SetClassWriter> _logger;

    public SetClassWriterTests() {
        var fileServiceMock = new Mock<IFileService>();
        fileServiceMock.Setup(f => f.PathJoin(It.IsAny<string?[]>())).Returns<string?[]>(Path.Join);
        _fileService = fileServiceMock.Object;

        _directoryService = Mock.Of<IDirectoryService>(d => d.DirectorySeparatorChar == '\\');
        _logger = Mock.Of<ILogger<SetClassWriter>>();
    }

    [Fact]
    public void GetRelativePath_GivenATargetParent_ShouldReturnTargetPath() {
        //assemble
        var writer = new SetClassWriter(_fileService, _logger, _directoryService);
        var expectedStatus = true;
        var targetFolder = "Generated\\TestType";
        var expectedPath = $"C:\\code\\{SolutionFolderName}\\{targetFolder}";
        var currentAssemblyPath = $"C:\\code\\{SolutionFolderName}\\Generator\\bin\\Debug\\net6.0";

        Mock.Get(_directoryService).Setup(d => d.GetCurrentDirectory()).Returns(currentAssemblyPath);

        //act
        var (success, path) = writer.TryGetAbsolutePathFromCurrentApp(SolutionFolderName, targetFolder);

        //assert
        success.Should().BeTrue();
        path.Should().Be(expectedPath);
    }

    [Fact]
    public void GetRelativePath_GivenCurrentPathIsRoot_ShouldReturnTargetPath() {
        //assemble
        var writer = new SetClassWriter(_fileService, _logger, _directoryService);
        var expectedStatus = true;
        var targetFolder = "Generated\\TestType";
        var expectedPath = $"C:\\code\\{SolutionFolderName}\\{targetFolder}";
        var currentAssemblyPath = $"C:\\code\\{SolutionFolderName}";

        Mock.Get(_directoryService).Setup(d => d.GetCurrentDirectory()).Returns(currentAssemblyPath);

        //act
        var (success, path) = writer.TryGetAbsolutePathFromCurrentApp(SolutionFolderName, targetFolder);

        //assert
        success.Should().BeTrue();
        path.Should().Be(expectedPath);
    }

    [Fact]
    public void GetRelativePath_GivenTargetNotInHeirarchy_ShouldReturnFalseAndNull() {
        //assemble
        var writer = new SetClassWriter(_fileService, _logger, _directoryService);
        var expectedStatus = true;
        var targetFolder = "Generated\\TestType";
        var currentAssemblyPath = "C:\\code\\SomeWeirdFolder\\With\\Lots\\Of\\Folders";

        Mock.Get(_directoryService).Setup(d => d.GetCurrentDirectory()).Returns(currentAssemblyPath);

        //act
        var (success, path) = writer.TryGetAbsolutePathFromCurrentApp(SolutionFolderName, targetFolder);

        //assert
        success.Should().BeFalse();
        path.Should().BeNull();
    }
}