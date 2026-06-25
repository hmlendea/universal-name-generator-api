using System.Collections.Generic;
using System.Linq;

using NuciGenerators.Text.Models;

using UniversalNameGenerator.API.DataAccess.DataObjects;

namespace UniversalNameGenerator.API.Service.Mapping
{
    static class WordMappingExtensions
    {
        internal static Word ToDomainModel(this WordEntity entity) => new()
        {
            Id = entity.Id,
            Values = entity.Values
        };

        internal static IEnumerable<Word> ToDomainModels(this IEnumerable<WordEntity> entities)
            => entities.Select(e => e.ToDomainModel());
    }
}
