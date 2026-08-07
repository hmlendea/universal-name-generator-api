using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;

using UniversalNameGenerator.API.Configuration;
using UniversalNameGenerator.API.Models;
using UniversalNameGenerator.API.Service;

namespace UniversalNameGenerator.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public sealed class NamesController(
        INameGeneratorService nameGeneratorService,
        SecuritySettings securitySettings) : NuciApiController, INamesController
    {
        private readonly NuciApiAuthorisation authorisation = NuciApiAuthorisation.ApiKey(securitySettings.ApiKey);

        [HttpGet]
        public ActionResult GetNames([FromQuery] GetNamesRequest request)
            => ProcessRequest(
                request,
                () => new GetNamesResponse { Names = nameGeneratorService.GetNames(request.Schema, request.Count) },
                authorisation);
    }
}
