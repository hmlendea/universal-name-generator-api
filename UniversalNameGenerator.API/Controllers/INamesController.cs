using Microsoft.AspNetCore.Mvc;

using UniversalNameGenerator.API.Models;

namespace UniversalNameGenerator.API.Controllers
{
    public interface INamesController
    {
        ActionResult GetNames(GetNamesRequest request);
    }
}
