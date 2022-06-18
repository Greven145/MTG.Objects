using Microsoft.Extensions.DependencyInjection;
using MTG.Object.Generator.Modules.EnumGenerator.HostedServices;
using MTG.Object.Generator.Modules.EnumGenerator.Interfaces;
using MTG.Object.Generator.Modules.EnumGenerator.Services;
using MTG.Object.Generator.Modules.Shared.Interfaces;
using MTG.Object.Generator.Modules.Shared.Services;

namespace MTG.Object.Generator.Modules.EnumGenerator; 

internal class ModuleInitializer : IModule {
    public void Initialize(IServiceCollection services) {
        services.AddScoped<IJsonParser, JsonParser>();
        services.AddScoped<IEnumProcessor, EnumProcessor>();
        services.AddScoped<IEnumCodeGenerator, EnumCodeGenerator>();
        services.AddScoped<IEnumClassWriter, EnumClassWriter>();

        services.AddHostedService<EnumGeneratorService>();
    }
}