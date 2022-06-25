using MTG.Object.Generator.Modules.Shared.Interfaces;

namespace MTG.Object.Generator.Modules.Shared.Services;

public class DirectoryService : IDirectoryService {
    public string GetCurrentDirectory() => Directory.GetCurrentDirectory();
    public char DirectorySeparatorChar => Path.DirectorySeparatorChar;
}