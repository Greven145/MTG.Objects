namespace MTG.Object.Generator.Modules.Shared.Interfaces;

internal interface IDirectoryService {
    char DirectorySeparatorChar { get; }
    string GetCurrentDirectory();
}