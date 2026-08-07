using System;
using System.Collections.Generic;

using NuciDAL.DataObjects;

namespace UniversalNameGenerator.API.DataAccess.DataObjects
{
    public sealed class WordDataObject : EntityBase, IEquatable<WordDataObject>
    {
        public List<string> Values { get; set; }

        public WordDataObject() => Values = [];

        public bool Equals(WordDataObject other)
        {
            if (other is null)
            {
                return false;
            }

            return string.Equals(Id, other.Id, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
            => obj is WordDataObject wordDataObject && Equals(wordDataObject);

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
