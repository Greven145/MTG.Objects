using Microsoft.Extensions.Hosting;
using MTG.Object.Generator.Modules;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => { services.AddModules(); })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();