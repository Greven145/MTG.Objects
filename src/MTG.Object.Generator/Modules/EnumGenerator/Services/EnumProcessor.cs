using Ardalis.GuardClauses;
using MTG.Object.Generator.Modules.EnumGenerator.Interfaces;
using MTG.Object.Generator.Modules.EnumGenerator.Models;
using Newtonsoft.Json.Linq;

namespace MTG.Object.Generator.Modules.EnumGenerator.Services;

internal class EnumProcessor : IEnumProcessor {
    public IEnumerable<Category> ProcessCategories(JProperty data) {
        var categories = new List<Category>();

        foreach (var cat in data.Values().Cast<JProperty>()) {
            Guard.Against.Null(cat, nameof(cat));
            var types = GetTypesFromCategory(cat.Values());
            var category = new Category(cat.Name, types);
            categories.Add(category);
        }

        return categories;
    }

    private static IEnumerable<EnumType> GetTypesFromCategory(IEnumerable<JToken> categoryChildren) {
        var types = new List<EnumType>();

        foreach (var childToken in categoryChildren) {
            var enumTypeToken = ValidateTokenIsJProperty(childToken);
            var firstValue = enumTypeToken.First ??
                             throw new InvalidDataException("Expected data is missing from JSON");
            var childTokens = firstValue.Children<JToken>();
            var childNames = childTokens.Select(j =>
                j.Value<string>() ?? throw new InvalidDataException("Token expected to be string"));

            var newType = new EnumType(enumTypeToken.Name, childNames);
            types.Add(newType);
        }

        return types;
    }

    private static JProperty ValidateTokenIsJProperty(JToken childToken) {
        if (childToken is not JProperty enumTypeToken) {
            throw new InvalidDataException("Expected child token to be a property");
        }

        return enumTypeToken;
    }
}