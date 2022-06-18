using MTG.Object.Generator.Modules.Shared.Interfaces;
using Newtonsoft.Json.Linq;

namespace MTG.Object.Generator.Modules.Shared.Services;

internal class JsonParser : IJsonParser {
    public JProperty ParseJson(string content) {
        if (JToken.Parse(content, null) is not JObject enumData) {
            throw new InvalidDataException("The enum data was null");
        }

        return enumData.Properties().First(p => p.Name == "data");
    }
}