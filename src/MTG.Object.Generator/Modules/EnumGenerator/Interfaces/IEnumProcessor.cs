using MTG.Object.Generator.Modules.EnumGenerator.Models;
using Newtonsoft.Json.Linq;

namespace MTG.Object.Generator.Modules.EnumGenerator.Interfaces;

internal interface IEnumProcessor {
    IEnumerable<Category> ProcessCategories(JProperty data);
}