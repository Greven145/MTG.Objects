namespace MTG.Object.Generator.Modules.Shared.Interfaces.Bases;

internal interface IClassWriter {
    void RemoveFolderIfExists();

    ValueTask WriteClassFile(string className, string contents, CancellationToken cancellationToken,
        string? subFolder = null);
}