using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using UniversalNameGenerator.API.IntegrationTests.Assertions;
using UniversalNameGenerator.API.IntegrationTests.Infrastructure;

namespace UniversalNameGenerator.API.IntegrationTests.Controllers
{
    [TestFixture]
    public sealed class NamesControllerTests : NamesControllerTestBase
    {
        private static string Schema => "astora-settlements";

        private static int RequestedCount => 4;

        private static IEnumerable<string> ExpectedNames =>
        [
            "Solara",
            "Cluj-Napoca",
            "Oradea",
            "Nucilandia"
        ];

        [Test]
        public async Task GivenAValidRequest_WhenGettingNames_ThenTheGeneratedNamesAreReturned()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, RequestedCount))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(
                BuildQuery(Schema, RequestedCount));

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames(Schema, RequestedCount),
                Times.Once);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(32)]
        [TestCase(42)]
        [TestCase(48)]
        [TestCase(64)]
        [TestCase(96)]
        [TestCase(128)]
        [TestCase(256)]
        [TestCase(512)]
        [TestCase(613)]
        [TestCase(873)]
        [TestCase(1024)]
        [TestCase(2048)]
        [TestCase(4096)]
        [TestCase(8192)]
        [TestCase(9999)]
        [TestCase(50000)]
        [TestCase(99999)]
        [TestCase(100000)]
        public async Task GivenAValidCount_WhenGettingNames_ThenTheExactCountIsPassedToTheService(int count)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, count))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(BuildQuery(Schema, count));

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(service => service.GetNames(Schema, count), Times.Once);
        }

        [TestCase("arabic-toponyms")]
        [TestCase("astora-settlements")]
        [TestCase("city-selection")]
        [TestCase("A")]
        [TestCase("schema with spaces")]
        [TestCase("schema+plus")]
        [TestCase("schema&ampersand")]
        [TestCase("schema/slash")]
        [TestCase("schema?question")]
        [TestCase("schema#fragment")]
        [TestCase("schema=equals")]
        [TestCase("schema%percent")]
        [TestCase("schema_underscore")]
        [TestCase("schema.dot")]
        [TestCase("SCHEMA-UPPERCASE")]
        [TestCase("1234567890")]
        [TestCase("Cluj-Napoca")]
        [TestCase("Çupișan")]
        [TestCase("Ionuț")]
        [TestCase("日本語")]
        [TestCase("العربية")]
        [TestCase("emoji-☀")]
        [TestCase("leading-space ")]
        [TestCase(" trailing-space")]
        [TestCase("two  spaces")]
        [TestCase("'single-quotes'")]
        [TestCase("\"double-quotes\"")]
        [TestCase("brackets[42]")]
        [TestCase("parentheses(42)")]
        [TestCase("semicolon;colon:")]
        public async Task GivenAnEncodedSchema_WhenGettingNames_ThenTheExactSchemaIsPassedToTheService(
            string schema)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(schema, RequestedCount))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(
                BuildQuery(schema, RequestedCount));

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames(schema, RequestedCount),
                Times.Once);
        }

        [Test]
        public async Task GivenNoCount_WhenGettingNames_ThenTheDefaultCountIsOne()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, 1))
                .Returns(ExpectedNames);

            string query = $"schema={Uri.EscapeDataString(Schema)}";
            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(query);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(service => service.GetNames(Schema, 1), Times.Once);
        }

        [Test]
        public async Task GivenCaseVariedQueryNames_WhenGettingNames_ThenTheValuesAreBound()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, RequestedCount))
                .Returns(ExpectedNames);

            string query = $"SCHEMA={Uri.EscapeDataString(Schema)}&COUNT={RequestedCount}";
            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(query);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames(Schema, RequestedCount),
                Times.Once);
        }

        [Test]
        public async Task GivenAnAdditionalQueryParameter_WhenGettingNames_ThenItIsIgnored()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, RequestedCount))
                .Returns(ExpectedNames);

            string query = $"{BuildQuery(Schema, RequestedCount)}&unused=The%20cake%20is%20a%20lie";
            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(query);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames(Schema, RequestedCount),
                Times.Once);
        }

        [TestCase("token")]
        [TestCase("1234567890")]
        [TestCase("P%40ssw0rd%21")]
        [TestCase("0123456789abcdef0123456789abcdef")]
        [TestCase("ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789")]
        public async Task GivenAnHmacHeader_WhenGettingNames_ThenTheRequestRemainsValid(string hmacToken)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, RequestedCount))
                .Returns(ExpectedNames);
            using HttpRequestMessage request = new(
                HttpMethod.Get,
                $"/Names?{BuildQuery(Schema, RequestedCount)}");
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                $"Bearer {UniversalNameGeneratorApiFactory.ApiKey}");
            request.Headers.TryAddWithoutValidation("X-HMAC", hmacToken);

            using HttpResponseMessage response = await HttpClient.SendAsync(request);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames(Schema, RequestedCount),
                Times.Once);
        }

        [Test]
        public async Task GivenNoGeneratedNames_WhenGettingNames_ThenAnEmptyNamesArrayIsReturned()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, RequestedCount))
                .Returns([]);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(
                BuildQuery(Schema, RequestedCount));

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, []);
        }

        [Test]
        public async Task GivenDuplicateGeneratedNames_WhenGettingNames_ThenTheirOrderAndDuplicatesArePreserved()
        {
            IEnumerable<string> names = ["Yes Man", "Yes Man", "Grumpy Cat", "Yes Man"];
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, RequestedCount))
                .Returns(names);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(
                BuildQuery(Schema, RequestedCount));

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
        }

        [Test]
        public async Task GivenDiverseGeneratedNames_WhenGettingNames_ThenEveryValueIsPreserved()
        {
            IEnumerable<string> names =
            [
                string.Empty,
                " ",
                "Yes Man",
                "Ionuț Karr",
                "Çupișan",
                "日本語",
                "العربية",
                "☀",
                "quotes-'\"",
                "line one\nline two",
                "<script>alert('test')</script>"
            ];
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, RequestedCount))
                .Returns(names);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(
                BuildQuery(Schema, RequestedCount));

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
        }
    }
}