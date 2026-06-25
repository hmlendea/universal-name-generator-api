using System.ComponentModel.DataAnnotations;
using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace UniversalNameGenerator.API.Models
{
    public class GetNamesRequest : NuciApiRequest
    {
        [HmacOrder(1)]
        public string Schema { get; set; }

        [HmacOrder(2)]
        [Range(1, 100000)]
        public int Count { get; set; } = 1;
    }
}
