using NuciDAL.DataObjects;
using NuciExtensions;

namespace UniversalNameGenerator.API.DataAccess.DataObjects
{
    public class GenerationSchemaEntity : EntityBase
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public string Schema { get; set; }

        public string FilterlistPath { get; set; }

        public string WordCase { get; set; }

        public GenerationSchemaEntity()
            => WordCase = NuciGenerators.Text.Models.WordCase.Title.GetDisplayName();
    }
}
