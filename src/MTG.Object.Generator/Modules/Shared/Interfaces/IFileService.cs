using System.Diagnostics.CodeAnalysis;

namespace MTG.Object.Generator.Modules.Shared.Interfaces;

internal interface IFileService {
    public string GetFullPath(string path);
    public bool DirectoryExists([NotNullWhen(true)] string? path);
    public void DeleteDirectory(string path, bool recursive);
    public DirectoryInfo CreateDirectory(string path);
    public string PathJoin(params string?[] paths);
    public Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken);
}