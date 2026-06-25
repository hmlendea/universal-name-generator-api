using System.Collections.Generic;

using NuciDAL.DataObjects;

namespace UniversalNameGenerator.API.DataAccess.DataObjects
{
    public sealed class WordEntity : EntityBase
    {
        public ICollection<string> Values { get; set; }

        public WordEntity() => Values = [];
    }
}
