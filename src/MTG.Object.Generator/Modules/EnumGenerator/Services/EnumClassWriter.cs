using Microsoft.Extensions.Logging;
using MTG.Object.Generator.Modules.EnumGenerator.Interfaces;
using MTG.Object.Generator.Modules.Shared.Interfaces;
using MTG.Object.Generator.Modules.Shared.Services.Bases;

namespace MTG.Object.Generator.Modules.EnumGenerator.Services;

internal class EnumClassWriter : ClassWriter, IEnumClassWriter {
    public EnumClassWriter(IFileService fileService, ILogger<EnumClassWriter> logger) : base(fileService, logger) {
    }

    protected override string GetRelativePath() => "../../../../MTG.Objects/Enums/";
}