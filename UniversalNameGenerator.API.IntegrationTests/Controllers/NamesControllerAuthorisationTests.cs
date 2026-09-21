using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using NuciAPI.Responses;

using UniversalNameGenerator.API.IntegrationTests.Assertions;
using UniversalNameGenerator.API.IntegrationTests.Infrastructure;

namespace UniversalNameGenerator.API.IntegrationTests.Controllers
{
    [TestFixture]
    public sealed class NamesControllerAuthorisationTests : NamesControllerTestBase
    {
        private static string Query => "schema=astora-settlements&count=4";

        private static IEnumerable<string> ExpectedNames => ["Solaire of Astora"];

        [TestCase("NucileRullz!")]
        [TestCase(" NucileRullz!")]
        [TestCase("NucileRullz! ")]
        [TestCase("  NucileRullz!  ")]
        [TestCase("Bearer NucileRullz!")]
        [TestCase("bearer NucileRullz!")]
        [TestCase("BEARER NucileRullz!")]
        [TestCase("BeArEr NucileRullz!")]
        [TestCase("Bearer  NucileRullz!")]
        [TestCase("Bearer     NucileRullz!")]
        [TestCase("Bearer\tNucileRullz!")]
        [TestCase("  Bearer NucileRullz!  ")]
        public async Task GivenAValidAuthorisationHeader_WhenGettingNames_ThenTheRequestIsAuthorised(
            string authorisationHeaderValue)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendGetRequestAsync(
                Query,
                authorisationHeaderValue);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames("astora-settlements", 4),
                Times.Once);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("Bearer")]
        [TestCase("Bearer ")]
        [TestCase("Basic NucileRullz!")]
        [TestCase("ApiKey NucileRullz!")]
        [TestCase("Token NucileRullz!")]
        [TestCase("Nucilerullz!")]
        [TestCase("NUCILERULLZ!")]
        [TestCase("NucileRullz")]
        [TestCase("NucileRullz!!")]
        [TestCase("xNucileRullz!")]
        [TestCase("Bearer xNucileRullz!")]
        [TestCase("Bearer NucileRullz!!")]
        [TestCase("Bearer NucileRullz%21")]
        [TestCase("Bearer%20NucileRullz!")]
        [TestCase("Bearer: NucileRullz!")]
        [TestCase("Bearer, NucileRullz!")]
        [TestCase("TestPassword!")]
        [TestCase("1234567890")]
        public async Task GivenAnInvalidAuthorisationHeader_WhenGettingNames_ThenAuthenticationFails(
            string authorisationHeaderValue)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                Query,
                authorisationHeaderValue);

            await AssertAuthenticationFailureAsync(response);
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenNoAuthorisationHeader_WhenGettingNames_ThenAuthenticationFails()
        {
            using HttpResponseMessage response = await SendGetRequestAsync(Query, null);

            await AssertAuthenticationFailureAsync(response);
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenTheApiKeyOnlyAsAQueryParameter_WhenGettingNames_ThenAuthenticationFails()
        {
            string query = $"{Query}&apiKey={UniversalNameGeneratorApiFactory.ApiKey}";

            using HttpResponseMessage response = await SendGetRequestAsync(query, null);

            await AssertAuthenticationFailureAsync(response);
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        private static Task AssertAuthenticationFailureAsync(HttpResponseMessage response)
            => NuciApiResponseAssertions.AssertErrorResponseAsync(
                response,
                HttpStatusCode.Unauthorized,
                NuciApiResponseMessages.ErrorMessages.AuthenticationFailure,
                NuciApiResponseCodes.ErrorCodes.AuthenticationFailure);
    }
}