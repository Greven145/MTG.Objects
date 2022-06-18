using MTG.Object.Generator.Modules.Shared.Interfaces;

namespace MTG.Object.Generator.Modules.Shared.Services;

internal class FileService : IFileService {
    public string GetFullPath(string path) => Path.GetFullPath(path);
    public bool DirectoryExists(string? path) => Directory.Exists(path);
    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);
    public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);
    public void WriteAllText(string path, string? contents) => File.WriteAllText(path, contents);
    public string PathJoin(params string?[] paths) => Path.Join(paths);
    public string ReadAllText(string path) => File.ReadAllText(path);

    public Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken) =>
        File.WriteAllTextAsync(path, contents, cancellationToken);
}