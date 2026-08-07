using System;
using System.Collections.Generic;
using System.Linq;

using NuciGenerators.Text.Models;

using UniversalNameGenerator.API.DataAccess.DataObjects;

namespace UniversalNameGenerator.API.Service.Mapping
{
    static class GenerationSchemaMappingExtensions
    {
        internal static GenerationSchema ToDomainModel(this GenerationSchemaEntity entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
            Schema = entity.Schema,
            FilterlistPath = entity.FilterlistPath,
            WordCase = Enum.Parse<WordCase>(entity.WordCase),
        };

        internal static IEnumerable<GenerationSchema> ToDomainModels(this IEnumerable<GenerationSchemaEntity> entities)
            => entities.Select(e => e.ToDomainModel());
    }
}
