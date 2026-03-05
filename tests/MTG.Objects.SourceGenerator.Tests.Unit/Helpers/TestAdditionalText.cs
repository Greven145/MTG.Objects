using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace MTG.Objects.SourceGenerator.Tests.Unit.Helpers;

internal sealed class TestAdditionalText : AdditionalText
{
    private readonly string _text;

    public TestAdditionalText(string path, string text)
    {
        Path = path;
        _text = text;
    }

    public override string Path { get; }

    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return SourceText.From(_text, Encoding.UTF8);
    }
}
