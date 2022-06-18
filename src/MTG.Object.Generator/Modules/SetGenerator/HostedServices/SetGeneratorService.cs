using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using MTG.Object.Generator.Modules.SetGenerator.Interfaces;
using MTG.Object.Generator.Modules.Shared.HostedServices.Bases;
using MTG.Object.Generator.Modules.Shared.Interfaces;
using MTG.Object.Generator.Modules.Shared.Interfaces.Bases;

namespace MTG.Object.Generator.Modules.SetGenerator.HostedServices;

internal class SetGeneratorService : CompletableBackgroundService {
    private readonly IClassWriter _classWriter;
    private readonly IMTGJsonClient _client;
    private readonly ISetCodeGenerator _codeGenerator;

    public SetGeneratorService(IHostApplicationLifetime hostApplicationLifetime,
        ISetClassWriter classWriter, ISetCodeGenerator codeGenerator, IMTGJsonClient client) : base(
        hostApplicationLifetime) {
        _classWriter = Guard.Against.Null(classWriter);
        _codeGenerator = Guard.Against.Null(codeGenerator);
        _client = Guard.Against.Null(client);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        var classBuilder = new StringBuilder();

        _classWriter.RemoveFolderIfExists();
        var json = await _client.GetSets();
        _codeGenerator.Data = json.Data;
        _codeGenerator.GenerateHeader(classBuilder, "Sets");
        _codeGenerator.GenerateConstructor(classBuilder, "Sets");
        _codeGenerator.GenerateFooter(classBuilder);
        await _classWriter.WriteClassFile("Sets", classBuilder.ToString(), stoppingToken);
        await base.ExecuteAsync(stoppingToken);
    }
}