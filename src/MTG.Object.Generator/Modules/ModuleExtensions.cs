using Microsoft.Extensions.DependencyInjection;

namespace MTG.Object.Generator.Modules;

internal static class ModuleExtensions {
    public static IServiceCollection AddModules(this IServiceCollection services) {
        var modules = GetModules();

        foreach (var module in modules) {
            InitializeModule(services, module);
        }

        return services;
    }

    private static void InitializeModule(IServiceCollection services, Type module) {
        var instance = (IModule)Activator.CreateInstance(module)!;
        instance.Initialize(services);
    }

    private static IEnumerable<Type> GetModules() {
        var type = typeof(IModule);
        var assembly = type.Assembly;
        var typesInAssembly = assembly.GetTypes();
        var modules = typesInAssembly.Where(t => {
            var interfaces = t.GetInterfaces();
            return interfaces.Contains(type);
        });
        return modules;
    }
}