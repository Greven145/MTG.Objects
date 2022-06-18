using MTG.Object.Generator.Modules.SetGenerator.Models.MTGJson;
using MTG.Object.Generator.Modules.Shared.Interfaces.Bases;

namespace MTG.Object.Generator.Modules.SetGenerator.Interfaces;

internal interface ISetCodeGenerator : ICodeGenerator {
    IEnumerable<Datum>? Data { get; set; }
}