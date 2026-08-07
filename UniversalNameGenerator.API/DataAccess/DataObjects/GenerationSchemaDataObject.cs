using System;
using System.Xml.Serialization;

using NuciDAL.DataObjects;
using NuciExtensions;

namespace UniversalNameGenerator.API.DataAccess.DataObjects
{
    public sealed class GenerationSchemaDataObject : EntityBase, IEquatable<GenerationSchemaDataObject>
    {
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Schema { get; set; } = string.Empty;

        public string FilterlistPath { get; set; } = string.Empty;

        public string WordCase { get; set; }

        public GenerationSchemaDataObject()
            => WordCase = NuciGenerators.Text.Models.WordCase.Title.GetDisplayName();

        public bool Equals(GenerationSchemaDataObject other)
        {
            if (other is null)
            {
                return false;
            }

            return string.Equals(Id, other.Id, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
            => obj is GenerationSchemaDataObject generationSchemaDataObject && Equals(generationSchemaDataObject);

        public override int GetHashCode()
        {
            if (Id is null)
            {
                return StringComparer.Ordinal.GetHashCode(string.Empty);
            }

            return StringComparer.Ordinal.GetHashCode(Id);
        }
    }
}
