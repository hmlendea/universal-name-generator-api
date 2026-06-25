using System.Collections.Generic;

using UniversalNameGenerator.API.DataAccess.DataObjects;

namespace UniversalNameGenerator.API.DataAccess.Repositories
{
    public interface IWordRepository
    {
        IEnumerable<WordEntity> GetAll();
    }
}
