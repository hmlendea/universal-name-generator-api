using System.Collections.Generic;
using System.Linq;

using NuciGenerators.Text.Models;

using UniversalNameGenerator.API.DataAccess.DataObjects;

namespace UniversalNameGenerator.API.Service.Mappings
{
    internal static class WordMappingExtensions
    {
        internal static Word ToServiceModel(this WordDataObject wordDataObject) => new()
        {
            Id = wordDataObject.Id,
            Values = wordDataObject.Values
        };

        internal static IEnumerable<Word> ToServiceModels(this IEnumerable<WordDataObject> wordDataObjects)
            => wordDataObjects.Select(wordDataObject => wordDataObject.ToServiceModel());
    }
}
