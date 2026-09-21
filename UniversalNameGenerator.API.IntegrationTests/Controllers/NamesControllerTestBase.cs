using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using UniversalNameGenerator.API.IntegrationTests.Infrastructure;
using UniversalNameGenerator.API.Service;

namespace UniversalNameGenerator.API.IntegrationTests.Controllers
{
    public abstract class NamesControllerTestBase
    {
        private static string NamesRoute => "/Names";

        private UniversalNameGeneratorApiFactory applicationFactory = null!;

        protected HttpClient HttpClient { get; private set; } = null!;

        protected Mock<INameGeneratorService> NameGeneratorServiceMock { get; private set; } = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            applicationFactory = new UniversalNameGeneratorApiFactory();
            NameGeneratorServiceMock = applicationFactory.NameGeneratorServiceMock;
            HttpClient = applicationFactory.CreateClient();
        }

        [SetUp]
        public void SetUp()
            => NameGeneratorServiceMock.Reset();

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            HttpClient.Dispose();
            applicationFactory.Dispose();
        }

        protected Task<HttpResponseMessage> SendAuthorisedGetRequestAsync(string query)
            => SendGetRequestAsync(query, $"Bearer {UniversalNameGeneratorApiFactory.ApiKey}");

        protected Task<HttpResponseMessage> SendAuthorisedRequestAsync(
            HttpMethod method,
            string requestUri)
            => SendRequestAsync(
                method,
                requestUri,
                $"Bearer {UniversalNameGeneratorApiFactory.ApiKey}");

        protected async Task<HttpResponseMessage> SendGetRequestAsync(
            string query,
            string? authorisationHeaderValue)
        {
            string requestUri = NamesRoute;

            if (!string.IsNullOrEmpty(query))
            {
                requestUri += $"?{query}";
            }

            return await SendRequestAsync(HttpMethod.Get, requestUri, authorisationHeaderValue);
        }

        protected async Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string requestUri,
            string? authorisationHeaderValue)
        {
            using HttpRequestMessage request = new(method, requestUri);

            if (authorisationHeaderValue is not null)
            {
                request.Headers.TryAddWithoutValidation("Authorization", authorisationHeaderValue);
            }

            return await HttpClient.SendAsync(request);
        }

        protected static string BuildQuery(string schema, int count)
            => $"schema={Uri.EscapeDataString(schema)}&count={count.ToString(CultureInfo.InvariantCulture)}";
    }
}