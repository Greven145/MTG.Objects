using Microsoft.Extensions.DependencyInjection;

namespace MTG.Object.Generator.Modules;

internal interface IModule {
    void Initialize(IServiceCollection services);
}