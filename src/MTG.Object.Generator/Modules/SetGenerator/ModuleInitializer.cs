using Microsoft.Extensions.DependencyInjection;
using MTG.Object.Generator.Modules.SetGenerator.HostedServices;
using MTG.Object.Generator.Modules.SetGenerator.Interfaces;
using MTG.Object.Generator.Modules.SetGenerator.Services;

namespace MTG.Object.Generator.Modules.SetGenerator; 

internal class ModuleInitializer : IModule {
    public void Initialize(IServiceCollection services) {
        services.AddScoped<ISetCodeGenerator, SetCodeGenerator>();
        services.AddScoped<ISetClassWriter, SetClassWriter>();
        services.AddHostedService<SetGeneratorService>();
    }
}