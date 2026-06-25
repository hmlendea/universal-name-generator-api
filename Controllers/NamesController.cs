using Microsoft.AspNetCore.Mvc;
using NuciAPI.Controllers;
using UniversalNameGenerator.API.Models;
using UniversalNameGenerator.API.Configuration;
using UniversalNameGenerator.API.Service;

namespace UniversalNameGenerator.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NamesController(
        INameGeneratorService service,
        SecuritySettings securitySettings) : NuciApiController
    {
        readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpGet]
        public ActionResult GetNames([FromQuery] GetNamesRequest request)
            => ProcessRequest(
                request,
                () => new GetNamesResponse { Names = service.GetNames(request.Schema, request.Count) },
                authorisation);
    }
}
