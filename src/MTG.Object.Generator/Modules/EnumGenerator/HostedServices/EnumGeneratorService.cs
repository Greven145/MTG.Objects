using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using MTG.Object.Generator.Modules.EnumGenerator.Interfaces;
using MTG.Object.Generator.Modules.EnumGenerator.Models;
using MTG.Object.Generator.Modules.Shared.HostedServices.Bases;
using MTG.Object.Generator.Modules.Shared.Interfaces;
using MTG.Object.Generator.Modules.Shared.Interfaces.Bases;

namespace MTG.Object.Generator.Modules.EnumGenerator.HostedServices;

internal class EnumGeneratorService : CompletableBackgroundService {
    private readonly IClassWriter _classWriter;
    private readonly IMtgJsonClient _client;
    private readonly IEnumCodeGenerator _codeGenerator;
    private readonly IEnumProcessor _enumProcessor;
    private readonly IJsonParser _jsonParser;

    public EnumGeneratorService(IHostApplicationLifetime hostApplicationLifetime, IJsonParser jsonParser,
        IEnumClassWriter classWriter, IEnumProcessor enumProcessor, IEnumCodeGenerator codeGenerator,
        IMtgJsonClient client) : base(hostApplicationLifetime) {
        _client = client;
        _jsonParser = Guard.Against.Null(jsonParser);
        _classWriter = Guard.Against.Null(classWriter);
        _enumProcessor = Guard.Against.Null(enumProcessor);
        _codeGenerator = Guard.Against.Null(codeGenerator);
        _client = Guard.Against.Null(client);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _classWriter.RemoveFolderIfExists();
        var json = await _client.GetEnums();
        var data = _jsonParser.ParseJson(json);
        var categories = _enumProcessor.ProcessCategories(data);

        await ProcessCategories(categories);
        await base.ExecuteAsync(stoppingToken);
    }

    private async ValueTask ProcessCategories(IEnumerable<Category> categories) {
        foreach (var (name, types) in categories) {
            await ProcessEnumTypes(types, name);
        }
    }

    private async ValueTask ProcessEnumTypes(IEnumerable<EnumType> types, string name) {
        foreach (var (typeName, enums) in types) {
            await ProcessEnumType(name, typeName, enums);
        }
    }

    private async ValueTask ProcessEnumType(string name, string typeName, IEnumerable<string> enums) {
        var classBuilder = new StringBuilder();
        var values = new List<string>();
        var skippedOffset = 0;

        _codeGenerator.GenerateHeader(classBuilder, name, typeName);
        var enumValues = enums as string[] ?? enums.ToArray();
        for (var x = 0; x < enumValues.Length; x++) {
            _codeGenerator.FormatEnumName(enumValues.ElementAt(x), typeName, values, ref skippedOffset, classBuilder,
                x);
        }

        _codeGenerator.GenerateConstructor(classBuilder, typeName);
        _codeGenerator.GenerateFooter(classBuilder);
        await _classWriter.WriteClassFile(typeName, classBuilder.ToString(), CancellationToken.None, name);
    }
}