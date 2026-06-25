using System.Collections.Generic;
using System.Text.Json.Serialization;

using NuciAPI.Responses;

namespace UniversalNameGenerator.API.Models
{
    public class GetNamesResponse : NuciApiSuccessResponse
    {
        [JsonPropertyName("names")]
        public IEnumerable<string> Names { get; set; }
    }
}
