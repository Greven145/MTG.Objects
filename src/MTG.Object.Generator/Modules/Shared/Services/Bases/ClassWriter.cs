using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MTG.Object.Generator.Modules.Shared.Interfaces;

namespace MTG.Object.Generator.Modules.Shared.Services.Bases;

internal abstract class ClassWriter {
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;
    private readonly ILogger<ClassWriter> _logger;
    private string TargetFolder => _fileService.GetFullPath(_fileService.GetFullPath(GetRelativePath()));

    protected ClassWriter(IFileService fileService, ILogger<ClassWriter> logger, IDirectoryService directoryService) {
        _fileService = Guard.Against.Null(fileService);
        _logger = Guard.Against.Null(logger);
        _directoryService = Guard.Against.Null(directoryService);
    }

    public void RemoveFolderIfExists() {
        if (!_fileService.DirectoryExists(TargetFolder)) {
            return;
        }

        _logger.LogInformation("Deleting {_targetFolder}", TargetFolder);
        _fileService.DeleteDirectory(TargetFolder, true);
    }

    public virtual async ValueTask WriteClassFile(string className, string contents,
        CancellationToken cancellationToken, string? subFolder = null) {
        Guard.Against.NullOrEmpty(className, nameof(className));
        Guard.Against.NullOrEmpty(contents, nameof(contents));

        var targetFolder = HandleSubFolder(subFolder);
        CreateDirectoryIfNecessary(targetFolder);
        await WriteClassToFile(className, contents, targetFolder, cancellationToken);
    }

    public (bool success, string? path) TryGetAbsolutePathFromCurrentApp(string projectRoot, string pathFromRoot) {
        var currentDirectory = new DirectoryInfo(_directoryService.GetCurrentDirectory());

        return CheckDirectory(projectRoot, pathFromRoot, currentDirectory);
    }

    protected abstract string GetRelativePath();

    private async Task WriteClassToFile(string className, string contents,
        string targetFolder, CancellationToken cancellationToken) {
        var fileName = Path.Combine(targetFolder, $"{className}.cs");
        _logger.LogInformation("Writing contents to {fileName}", fileName);
        await _fileService.WriteAllTextAsync(fileName, contents, cancellationToken);
    }

    private void CreateDirectoryIfNecessary(string targetFolder) {
        if (_fileService.DirectoryExists(targetFolder)) {
            return;
        }

        _logger.LogInformation("Creating {targetCategoryFolder}", targetFolder);
        _fileService.CreateDirectory(targetFolder);
    }

    private string HandleSubFolder(string? subFolder) {
        var targetFolder = TargetFolder;
        if (subFolder is not null) {
            targetFolder = _fileService.PathJoin(TargetFolder, subFolder);
        }

        return targetFolder;
    }

    private (bool success, string? path) CheckDirectory(string projectRoot, string pathFromRoot,
        DirectoryInfo? currentDirectory) {
        if (currentDirectory is null) {
            return (false, null);
        }

        if (currentDirectory.Name == projectRoot) {
            return (true, _fileService.PathJoin(currentDirectory.FullName, pathFromRoot));
        }

        return CheckDirectory(projectRoot, pathFromRoot, currentDirectory.Parent);
    }
}