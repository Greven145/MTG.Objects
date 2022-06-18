using System.Text.Json;
using MTG.Object.Generator.Modules.SetGenerator.Models.MTGJson;
using MTG.Object.Generator.Modules.Shared.Constants;
using MTG.Object.Generator.Modules.Shared.Interfaces;

namespace MTG.Object.Generator.Modules.Shared.Services;

internal class MTGJsonClient : IMTGJsonClient {
    private const string ApiHost = "mtgjson.com";
    private const string ApiPath = "api";
    private const string ApiVersion = "v5";
    private static readonly Uri BaseUri;
    private readonly IHttpClientFactory _clientFactory;

    static MTGJsonClient() {
        BaseUri = new UriBuilder {
            Host = ApiHost,
            Scheme = Uri.UriSchemeHttps
        }.Uri;
    }

    public MTGJsonClient(IHttpClientFactory clientFactory) {
        _clientFactory = clientFactory;
    }

    public async ValueTask<string> GetEnums() {
        var client = GetClient();
        var uri = GetUri("EnumValues.json");
        return await client.GetStringAsync(uri);
    }

    public async ValueTask<SetRequestObject> GetSets() {
        var client = GetClient();
        var uri = GetUri("SetList.json");
        var response = await client.GetStringAsync(uri);
        return JsonSerializer.Deserialize<SetRequestObject>(response)!;
    }

    private static Uri GetUri(string fileName) =>
        new UriBuilder(BaseUri) {
            Path = $"{ApiPath}/{ApiVersion}/{fileName}"
        }.Uri;

    private HttpClient GetClient() => _clientFactory.CreateClient(HttpClientNames.MTGJson);
}