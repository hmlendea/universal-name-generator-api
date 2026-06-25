using System.Collections.Generic;

using NuciGenerators.Text.Models;

namespace UniversalNameGenerator.API.Service
{
    public interface INameGeneratorService
    {
        IEnumerable<string> GetNames(string schemaId, int count);

        IEnumerable<GenerationSchema> GetSchemas();
    }
}
