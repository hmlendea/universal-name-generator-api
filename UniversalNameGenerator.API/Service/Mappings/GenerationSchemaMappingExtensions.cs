using System;
using System.Collections.Generic;
using System.Linq;

using NuciGenerators.Text.Models;

using UniversalNameGenerator.API.DataAccess.DataObjects;

namespace UniversalNameGenerator.API.Service.Mappings
{
    internal static class GenerationSchemaMappingExtensions
    {
        internal static GenerationSchema ToServiceModel(this GenerationSchemaDataObject generationSchemaDataObject) => new()
        {
            Id = generationSchemaDataObject.Id,
            Name = generationSchemaDataObject.Name,
            Category = generationSchemaDataObject.Category,
            Schema = generationSchemaDataObject.Schema,
            FilterlistPath = generationSchemaDataObject.FilterlistPath,
            WordCase = Enum.Parse<WordCase>(generationSchemaDataObject.WordCase),
        };

        internal static IEnumerable<GenerationSchema> ToServiceModels(this IEnumerable<GenerationSchemaDataObject> generationSchemaDataObjects)
            => generationSchemaDataObjects.Select(generationSchemaDataObject => generationSchemaDataObject.ToServiceModel());
    }
}
