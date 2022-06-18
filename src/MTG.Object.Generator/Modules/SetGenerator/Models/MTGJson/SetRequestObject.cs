using System.Text.Json.Serialization;

namespace MTG.Object.Generator.Modules.SetGenerator.Models.MTGJson;

internal class SetRequestObject {
    [JsonPropertyName("data")] public Datum[] Data { get; set; } = default!;
}