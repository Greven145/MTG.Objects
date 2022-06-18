using MTG.Object.Generator.Modules.SetGenerator.Models.MTGJson;

namespace MTG.Object.Generator.Modules.Shared.Interfaces;

internal interface IMTGJsonClient {
    ValueTask<string> GetEnums();
    ValueTask<SetRequestObject> GetSets();
}