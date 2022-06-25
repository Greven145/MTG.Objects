using Microsoft.Extensions.Logging;
using MTG.Object.Generator.Modules.SetGenerator.Interfaces;
using MTG.Object.Generator.Modules.Shared.Interfaces;
using MTG.Object.Generator.Modules.Shared.Services.Bases;

namespace MTG.Object.Generator.Modules.SetGenerator.Services;

internal class SetClassWriter : ClassWriter, ISetClassWriter {
    public SetClassWriter(IFileService fileService, ILogger<SetClassWriter> logger, IDirectoryService directoryService)
        : base(fileService, logger, directoryService) {
    }

    public override ValueTask WriteClassFile(string className, string contents, CancellationToken cancellationToken,
        string? subFolder = null) =>
        base.WriteClassFile(className, contents, cancellationToken);

    protected override string GetRelativePath() =>
        TryGetAbsolutePathFromCurrentApp("MTG.Objects", "src/MTG.Objects/Sets").path ??
        throw new InvalidOperationException("Unable to get path to write sets to");
}