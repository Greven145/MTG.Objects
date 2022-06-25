using Microsoft.Extensions.Logging;
using MTG.Object.Generator.Modules.EnumGenerator.Interfaces;
using MTG.Object.Generator.Modules.Shared.Interfaces;
using MTG.Object.Generator.Modules.Shared.Services.Bases;

namespace MTG.Object.Generator.Modules.EnumGenerator.Services;

internal class EnumClassWriter : ClassWriter, IEnumClassWriter {
    public EnumClassWriter(IFileService fileService, ILogger<EnumClassWriter> logger,
        IDirectoryService directoryService) : base(fileService, logger, directoryService) {
    }

    protected override string GetRelativePath() =>
        TryGetAbsolutePathFromCurrentApp("MTG.Objects", "src/MTG.Objects/Enums").path ??
        throw new InvalidOperationException("Unable to get path to write Enums to");
}