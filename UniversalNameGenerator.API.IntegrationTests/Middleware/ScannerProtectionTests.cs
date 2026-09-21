using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using UniversalNameGenerator.API.IntegrationTests.Assertions;
using UniversalNameGenerator.API.IntegrationTests.Infrastructure;
using UniversalNameGenerator.API.Service;

namespace UniversalNameGenerator.API.IntegrationTests.Middleware
{
    [TestFixture]
    public sealed class ScannerProtectionTests
    {
        private static string AuthorisationHeaderValue
            => $"Bearer {UniversalNameGeneratorApiFactory.ApiKey}";

        private static string NamesRequestUri => "/Names?schema=astora-settlements&count=4";

        private static IEnumerable<string> ExpectedNames => ["Solara"];

        private UniversalNameGeneratorApiFactory applicationFactory = null!;
        private Mock<INameGeneratorService> nameGeneratorServiceMock = null!;
        private HttpClient httpClient = null!;

        [SetUp]
        public void SetUp()
        {
            applicationFactory = new UniversalNameGeneratorApiFactory();
            nameGeneratorServiceMock = applicationFactory.NameGeneratorServiceMock;
            httpClient = applicationFactory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            httpClient?.Dispose();
            applicationFactory?.Dispose();
        }

        [TestCase("/_ignition/execute-solution")]
        [TestCase("/_profiler")]
        [TestCase("/.aws/config")]
        [TestCase("/.aws/credentials")]
        [TestCase("/.env")]
        [TestCase("/.git/config")]
        [TestCase("/.npmrc")]
        [TestCase("/actuator/env")]
        [TestCase("/actuator/health")]
        [TestCase("/api-keys.txt")]
        [TestCase("/appsettings.json")]
        [TestCase("/config.json")]
        [TestCase("/credentials.json")]
        [TestCase("/database.sql")]
        [TestCase("/docker-compose.yml")]
        [TestCase("/docker-compose.yaml")]
        [TestCase("/env")]
        [TestCase("/graphql")]
        [TestCase("/local_settings.py")]
        [TestCase("/package.json")]
        [TestCase("/php_info")]
        [TestCase("/robots.txt")]
        [TestCase("/security.txt")]
        [TestCase("/serverless.yml")]
        [TestCase("/settings.json")]
        [TestCase("/storage/logs/error.log")]
        [TestCase("/terraform.tfstate")]
        [TestCase("/web.config")]
        [TestCase("/wp-admin/")]
        [TestCase("/backup.zip")]
        [TestCase("/index.php")]
        [TestCase("/index.php.bak")]
        [TestCase("/appsettings.Production.json")]
        [TestCase("/_next/static/chunk.js")]
        [TestCase("/console/admin")]
        public async Task GivenAForbiddenResourcePath_WhenSendingARequest_ThenTheClientIsForbidden(
            string path)
        {
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, path);

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await AssertForbiddenWithoutContentAsync(response);
            nameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase("Chrome/143.0.0.0")]
        [TestCase("Mozilla/5.0 Chrome/143.0.0.0 Safari/537.36")]
        [TestCase("chrome/143.0.0.0")]
        [TestCase("InternetMeasurement")]
        [TestCase("internetmeasurement-client")]
        [TestCase("OAI-SearchBot")]
        [TestCase("oai-searchbot/1.0")]
        [TestCase("SecurityScanner")]
        [TestCase("securityscanner/42")]
        public async Task GivenAForbiddenUserAgent_WhenGettingNames_ThenTheClientIsForbidden(
            string userAgent)
        {
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, NamesRequestUri);
            request.Headers.TryAddWithoutValidation("User-Agent", userAgent);

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await AssertForbiddenWithoutContentAsync(response);
            nameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(".Not/A)Brand")]
        [TestCase("\"Chromium\";v=\"142\", \".Not/A)Brand\";v=\"99\"")]
        [TestCase(".not/a)brand")]
        public async Task GivenAForbiddenClientHint_WhenGettingNames_ThenTheClientIsForbidden(
            string clientHint)
        {
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, NamesRequestUri);
            request.Headers.TryAddWithoutValidation("sec-ch-ua", clientHint);

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await AssertForbiddenWithoutContentAsync(response);
            nameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase("oai-searchbot(at)openai.com")]
        [TestCase("OAI-SEARCHBOT(AT)OPENAI.COM")]
        public async Task GivenAForbiddenFromHeader_WhenGettingNames_ThenTheClientIsForbidden(
            string fromHeaderValue)
        {
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, NamesRequestUri);
            request.Headers.TryAddWithoutValidation("From", fromHeaderValue);

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await AssertForbiddenWithoutContentAsync(response);
            nameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase("page=gravitysmtp-settings")]
        [TestCase("rest_route=/wp/v2/users")]
        [TestCase("rest_route=/wp/v2/users/")]
        [TestCase("XDEBUG_SESSION_START=phpstorm")]
        public async Task GivenAForbiddenQueryPattern_WhenGettingNames_ThenTheClientIsForbidden(
            string forbiddenQuery)
        {
            string requestUri = $"{NamesRequestUri}&{forbiddenQuery}";
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, requestUri);

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await AssertForbiddenWithoutContentAsync(response);
            nameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenAnEmptyRootRequest_WhenSendingTheRequest_ThenTheClientIsForbidden()
        {
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, "/");

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await AssertForbiddenWithoutContentAsync(response);
        }

        [Test]
        public async Task GivenARootRequestWithAQuery_WhenSendingTheRequest_ThenTheScannerPermitsIt()
        {
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, "/?probe=true");

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GivenARootRequestWithABody_WhenSendingTheRequest_ThenTheScannerPermitsIt()
        {
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Post, "/");
            request.Content = new StringContent("The cake is a lie", Encoding.UTF8, "text/plain");

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [TestCase("OPTIONS")]
        [TestCase("HEAD")]
        [TestCase("TRACE")]
        [TestCase("CONNECT")]
        public async Task GivenAnUnsafeRootMethod_WhenSendingTheRequest_ThenTheClientIsForbidden(
            string methodName)
        {
            using HttpRequestMessage request = CreateAuthorisedRequest(new HttpMethod(methodName), "/?probe=true");

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await AssertForbiddenWithoutContentAsync(response);
        }

        [TestCase("X-Forwarded-For", "198.51.100.42")]
        [TestCase("X-Forwarded-For", "unknown, 203.0.113.8")]
        [TestCase("X-Forwarded-For", "198.51.100.42:8080")]
        [TestCase("Forwarded", "for=198.51.100.42")]
        [TestCase("Forwarded", "for=unknown;proto=https, for=203.0.113.8")]
        [TestCase("Forwarded", "for=\"[2001:db8::1]:443\"")]
        [TestCase("X-Real-IP", "192.0.2.42")]
        [TestCase("CF-Connecting-IP", "2001:db8::42")]
        [TestCase("True-Client-IP", "203.0.113.42")]
        public async Task GivenAValidForwardedAddress_WhenGettingNames_ThenTheRequestIsPermitted(
            string headerName,
            string headerValue)
        {
            nameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Returns(ExpectedNames);
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, NamesRequestUri);
            request.Headers.TryAddWithoutValidation(headerName, headerValue);

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
        }

        [TestCase("unknown")]
        [TestCase("not-an-ip-address")]
        [TestCase("999.999.999.999")]
        [TestCase("198.51.100.42:invalid-port")]
        public async Task GivenAnInvalidForwardedAddress_WhenGettingNames_ThenTheRemoteAddressIsUsed(
            string headerValue)
        {
            nameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Returns(ExpectedNames);
            using HttpRequestMessage request = CreateAuthorisedRequest(HttpMethod.Get, NamesRequestUri);
            request.Headers.TryAddWithoutValidation("X-Forwarded-For", headerValue);

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
        }

        [Test]
        public async Task GivenAClientThatTriggeredProtection_WhenSendingAnotherRequest_ThenItRemainsForbidden()
        {
            using HttpRequestMessage forbiddenRequest = CreateAuthorisedRequest(
                HttpMethod.Get,
                "/appsettings.json");
            using HttpResponseMessage forbiddenResponse = await httpClient.SendAsync(forbiddenRequest);

            using HttpRequestMessage subsequentRequest = CreateAuthorisedRequest(HttpMethod.Get, NamesRequestUri);
            using HttpResponseMessage subsequentResponse = await httpClient.SendAsync(subsequentRequest);

            await AssertForbiddenWithoutContentAsync(forbiddenResponse);
            await AssertForbiddenWithoutContentAsync(subsequentResponse);
            nameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenOneBannedAddress_WhenAnotherAddressRequestsNames_ThenItIsPermitted()
        {
            nameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Returns(ExpectedNames);
            using HttpRequestMessage forbiddenRequest = CreateAuthorisedRequest(
                HttpMethod.Get,
                "/appsettings.json");
            forbiddenRequest.Headers.TryAddWithoutValidation("X-Forwarded-For", "198.51.100.42");
            using HttpResponseMessage forbiddenResponse = await httpClient.SendAsync(forbiddenRequest);

            using HttpRequestMessage permittedRequest = CreateAuthorisedRequest(HttpMethod.Get, NamesRequestUri);
            permittedRequest.Headers.TryAddWithoutValidation("X-Forwarded-For", "203.0.113.42");
            using HttpResponseMessage permittedResponse = await httpClient.SendAsync(permittedRequest);

            await AssertForbiddenWithoutContentAsync(forbiddenResponse);
            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(
                permittedResponse,
                ExpectedNames);
        }

        private static HttpRequestMessage CreateAuthorisedRequest(HttpMethod method, string requestUri)
        {
            HttpRequestMessage request = new(method, requestUri);
            request.Headers.TryAddWithoutValidation("Authorization", AuthorisationHeaderValue);

            return request;
        }

        private static async Task AssertForbiddenWithoutContentAsync(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(responseContent, Is.Empty);
            });
        }
    }
}