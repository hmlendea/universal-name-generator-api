using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using UniversalNameGenerator.API.IntegrationTests.Assertions;

namespace UniversalNameGenerator.API.IntegrationTests.Controllers
{
    [TestFixture]
    public sealed class NamesControllerRoutingTests : NamesControllerTestBase
    {
        private static IEnumerable<string> ExpectedNames => ["Solara"];

        [TestCase("/Names")]
        [TestCase("/names")]
        [TestCase("/NAMES")]
        [TestCase("/NaMeS")]
        [TestCase("/Names/")]
        [TestCase("/names/")]
        public async Task GivenAValidRouteVariant_WhenGettingNames_ThenTheEndpointIsMatched(
            string route)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedRequestAsync(
                HttpMethod.Get,
                $"{route}?schema=astora-settlements&count=4");

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames("astora-settlements", 4),
                Times.Once);
        }

        [TestCase("/Name")]
        [TestCase("/Names/extra")]
        [TestCase("/api/Names")]
        [TestCase("/v1/Names")]
        [TestCase("/unknown")]
        [TestCase("/health")]
        [TestCase("/favicon.ico")]
        [TestCase("/names.json")]
        [TestCase("/Names.txt")]
        [TestCase("/Names/42")]
        public async Task GivenAnUnmatchedRoute_WhenSendingARequest_ThenNotFoundIsReturned(
            string route)
        {
            using HttpResponseMessage response = await SendAuthorisedRequestAsync(HttpMethod.Get, route);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase("POST")]
        [TestCase("PUT")]
        [TestCase("PATCH")]
        [TestCase("DELETE")]
        [TestCase("HEAD")]
        [TestCase("OPTIONS")]
        [TestCase("TRACE")]
        [TestCase("CONNECT")]
        public async Task GivenAnUnsupportedHttpMethod_WhenRequestingNames_ThenMethodNotAllowedIsReturned(
            string methodName)
        {
            HttpMethod method = new(methodName);

            using HttpResponseMessage response = await SendAuthorisedRequestAsync(
                method,
                "/Names?schema=astora-settlements&count=4");

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
                Assert.That(
                    response.Content.Headers.Allow,
                    Does.Contain(HttpMethod.Get.Method));
            });
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }
    }
}