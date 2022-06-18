using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MTG.Object.Generator.Modules.Shared.Interfaces;

namespace MTG.Object.Generator.Modules.Shared.Services.Bases;

internal abstract class ClassWriter {
    private readonly IFileService _fileService;
    private readonly ILogger<ClassWriter> _logger;
    private string TargetFolder => _fileService.GetFullPath(_fileService.GetFullPath(GetRelativePath()));

    protected ClassWriter(IFileService fileService, ILogger<ClassWriter> logger) {
        _fileService = Guard.Against.Null(fileService, nameof(fileService));
        _logger = Guard.Against.Null(logger, nameof(logger));
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
        await WriteClassToFile(className, contents, cancellationToken, targetFolder);
    }

    private async Task WriteClassToFile(string className, string contents, CancellationToken cancellationToken,
        string targetFolder) {
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

    protected abstract string GetRelativePath();
}