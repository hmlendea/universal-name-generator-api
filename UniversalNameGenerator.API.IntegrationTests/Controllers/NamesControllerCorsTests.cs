using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using UniversalNameGenerator.API.IntegrationTests.Assertions;
using UniversalNameGenerator.API.IntegrationTests.Infrastructure;

namespace UniversalNameGenerator.API.IntegrationTests.Controllers
{
    [TestFixture]
    public sealed class NamesControllerCorsTests : NamesControllerTestBase
    {
        private static string AccessControlAllowHeadersName => "Access-Control-Allow-Headers";

        private static string AccessControlAllowMethodsName => "Access-Control-Allow-Methods";

        private static string AccessControlAllowOriginName => "Access-Control-Allow-Origin";

        private static string AccessControlRequestHeadersName => "Access-Control-Request-Headers";

        private static string AccessControlRequestMethodName => "Access-Control-Request-Method";

        private static string OriginHeaderName => "Origin";

        private static IEnumerable<string> ExpectedNames => ["Solara"];

        [TestCase("http://localhost:5000")]
        [TestCase("https://localhost:5001")]
        [TestCase("http://localhost:7000")]
        [TestCase("https://localhost:7001")]
        [TestCase("http://localhost:8080")]
        [TestCase("http://localhost:8081")]
        public async Task GivenAnAllowedOrigin_WhenGettingNames_ThenTheOriginIsPermitted(
            string origin)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Returns(ExpectedNames);
            using HttpRequestMessage request = CreateNamesRequest(HttpMethod.Get);
            request.Headers.TryAddWithoutValidation(OriginHeaderName, origin);

            using HttpResponseMessage response = await HttpClient.SendAsync(request);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            Assert.That(response.Headers.Contains(AccessControlAllowOriginName));
            Assert.That(
                response.Headers.GetValues(AccessControlAllowOriginName),
                Is.EqualTo(new[] { origin }));
        }

        [TestCase("http://localhost:5001")]
        [TestCase("https://localhost:5000")]
        [TestCase("http://localhost:7001")]
        [TestCase("https://localhost:7000")]
        [TestCase("https://localhost:8080")]
        [TestCase("https://localhost:8081")]
        [TestCase("http://localhost:42")]
        [TestCase("https://dummy-domain.com")]
        [TestCase("https://test.url.ro")]
        [TestCase("null")]
        public async Task GivenADisallowedOrigin_WhenGettingNames_ThenNoCorsOriginHeaderIsReturned(
            string origin)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Returns(ExpectedNames);
            using HttpRequestMessage request = CreateNamesRequest(HttpMethod.Get);
            request.Headers.TryAddWithoutValidation(OriginHeaderName, origin);

            using HttpResponseMessage response = await HttpClient.SendAsync(request);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            Assert.That(response.Headers.Contains(AccessControlAllowOriginName), Is.False);
        }

        [TestCase("http://localhost:5000")]
        [TestCase("https://localhost:5001")]
        [TestCase("http://localhost:7000")]
        [TestCase("https://localhost:7001")]
        [TestCase("http://localhost:8080")]
        [TestCase("http://localhost:8081")]
        public async Task GivenAnAllowedOrigin_WhenSendingAPreflightRequest_ThenCorsHeadersAreReturned(
            string origin)
        {
            using HttpRequestMessage request = CreatePreflightRequest(origin, HttpMethod.Get.Method);

            using HttpResponseMessage response = await HttpClient.SendAsync(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
                Assert.That(
                    response.Headers.GetValues(AccessControlAllowOriginName),
                    Is.EqualTo(new[] { origin }));
                Assert.That(
                    response.Headers.GetValues(AccessControlAllowMethodsName),
                    Does.Contain(HttpMethod.Get.Method));
            });
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase("GET")]
        [TestCase("POST")]
        [TestCase("PUT")]
        [TestCase("PATCH")]
        [TestCase("DELETE")]
        [TestCase("HEAD")]
        [TestCase("OPTIONS")]
        [TestCase("TRACE")]
        public async Task GivenAnyRequestedMethod_WhenSendingAnAllowedPreflightRequest_ThenTheMethodIsPermitted(
            string requestedMethod)
        {
            using HttpRequestMessage request = CreatePreflightRequest(
                "http://localhost:5000",
                requestedMethod);

            using HttpResponseMessage response = await HttpClient.SendAsync(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
                Assert.That(
                    response.Headers.GetValues(AccessControlAllowMethodsName),
                    Does.Contain(requestedMethod));
            });
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenRequestedHeaders_WhenSendingAnAllowedPreflightRequest_ThenAllHeadersArePermitted()
        {
            IEnumerable<string> requestedHeaders = ["Authorization", "Content-Type", "X-HMAC", "X-Custom"];
            using HttpRequestMessage request = CreatePreflightRequest(
                "http://localhost:5000",
                HttpMethod.Get.Method);
            request.Headers.TryAddWithoutValidation(
                AccessControlRequestHeadersName,
                string.Join(", ", requestedHeaders));

            using HttpResponseMessage response = await HttpClient.SendAsync(request);
            IEnumerable<string> permittedHeaders = response.Headers
                .GetValues(AccessControlAllowHeadersName)
                .SelectMany(value => value.Split(','))
                .Select(value => value.Trim());

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
                Assert.That(permittedHeaders, Is.EquivalentTo(requestedHeaders));
            });
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase("http://localhost:5001")]
        [TestCase("https://localhost:5000")]
        [TestCase("https://dummy-domain.com")]
        [TestCase("https://test.url.ro")]
        public async Task GivenADisallowedOrigin_WhenSendingAPreflightRequest_ThenNoCorsHeadersAreReturned(
            string origin)
        {
            using HttpRequestMessage request = CreatePreflightRequest(origin, HttpMethod.Get.Method);

            using HttpResponseMessage response = await HttpClient.SendAsync(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
                Assert.That(response.Headers.Contains(AccessControlAllowOriginName), Is.False);
                Assert.That(response.Headers.Contains(AccessControlAllowMethodsName), Is.False);
            });
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        private static HttpRequestMessage CreateNamesRequest(HttpMethod method)
        {
            HttpRequestMessage request = new(
                method,
                "/Names?schema=astora-settlements&count=4");
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                $"Bearer {UniversalNameGeneratorApiFactory.ApiKey}");

            return request;
        }

        private static HttpRequestMessage CreatePreflightRequest(
            string origin,
            string requestedMethod)
        {
            HttpRequestMessage request = new(HttpMethod.Options, "/Names");
            request.Headers.TryAddWithoutValidation(OriginHeaderName, origin);
            request.Headers.TryAddWithoutValidation(AccessControlRequestMethodName, requestedMethod);

            return request;
        }
    }
}