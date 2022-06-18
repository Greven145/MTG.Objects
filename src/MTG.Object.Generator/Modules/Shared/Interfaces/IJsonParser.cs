using Newtonsoft.Json.Linq;

namespace MTG.Object.Generator.Modules.Shared.Interfaces;

internal interface IJsonParser {
    public JProperty ParseJson(string content);
}