using System.Text.Json.Serialization;

namespace MTG.Object.Generator.Modules.SetGenerator.Models.MTGJson;

internal class Datum {
    [JsonPropertyName("code")] public string Code { get; set; } = default!;

    [JsonPropertyName("name")] public string Name { get; set; } = default!;
}