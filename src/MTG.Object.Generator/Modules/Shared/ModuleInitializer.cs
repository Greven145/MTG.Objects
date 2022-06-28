using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MTG.Object.Generator.Modules.Shared.Constants;
using MTG.Object.Generator.Modules.Shared.Interfaces;
using MTG.Object.Generator.Modules.Shared.Policies;
using MTG.Object.Generator.Modules.Shared.Services;

namespace MTG.Object.Generator.Modules.Shared;

[UsedImplicitly]
internal class ModuleInitializer : IModule {
    public void Initialize(IServiceCollection services) {
        services.AddSingleton<ClientPolicy>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IDirectoryService, DirectoryService>();
        services.AddScoped<IMTGJsonClient, MTGJsonClient>();
        services
            .AddHttpClient(HttpClientNames.MTGJson)
            .AddPolicyHandler((provider, _) => provider.GetService<ClientPolicy>()?.ExponentialBackOff);
    }
}