using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using UniversalNameGenerator.API.IntegrationTests.Assertions;

namespace UniversalNameGenerator.API.IntegrationTests.Controllers
{
    [TestFixture]
    public sealed class NamesControllerValidationTests : NamesControllerTestBase
    {
        private static string Schema => "astora-settlements";

        private static IEnumerable<string> ExpectedNames => ["Solara"];

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-4)]
        [TestCase(-8)]
        [TestCase(-16)]
        [TestCase(-32)]
        [TestCase(-42)]
        [TestCase(-128)]
        [TestCase(-1024)]
        [TestCase(-8192)]
        [TestCase(-100000)]
        [TestCase(-100001)]
        [TestCase(int.MinValue)]
        [TestCase(100001)]
        [TestCase(100002)]
        [TestCase(131072)]
        [TestCase(1000000)]
        [TestCase(int.MaxValue)]
        public async Task GivenAnOutOfRangeCount_WhenGettingNames_ThenAValidationErrorIsReturned(int count)
        {
            string query = $"schema={Schema}&count={count.ToString(CultureInfo.InvariantCulture)}";

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(query);

            await NuciApiResponseAssertions.AssertValidationErrorResponseAsync(response, "Count");
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        [TestCase("one")]
        [TestCase("four")]
        [TestCase("true")]
        [TestCase("false")]
        [TestCase("null")]
        [TestCase("1.0")]
        [TestCase("3.14")]
        [TestCase("1,000")]
        [TestCase("1e2")]
        [TestCase("1E5")]
        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("-Infinity")]
        [TestCase("0b100")]
        [TestCase("4_2")]
        [TestCase("++1")]
        [TestCase("--1")]
        [TestCase("+-1")]
        [TestCase("42names")]
        [TestCase("四十二")]
        [TestCase("٤٢")]
        [TestCase("2147483648")]
        [TestCase("-2147483649")]
        [TestCase("999999999999999999999999999999999999")]
        [TestCase("[]")]
        [TestCase("{}")]
        public async Task GivenAMalformedCount_WhenGettingNames_ThenAValidationErrorIsReturned(
            string countValue)
        {
            string query = $"schema={Schema}&count={Uri.EscapeDataString(countValue)}";

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(query);

            await NuciApiResponseAssertions.AssertValidationErrorResponseAsync(response, "Count");
            NameGeneratorServiceMock.VerifyNoOtherCalls();
        }

        [TestCase("0001", 1)]
        [TestCase("000004", 4)]
        [TestCase("+8", 8)]
        [TestCase(" 16", 16)]
        [TestCase("32 ", 32)]
        [TestCase(" 42 ", 42)]
        [TestCase("0x10", 16)]
        [TestCase("00000128", 128)]
        [TestCase("+100000", 100000)]
        public async Task GivenAValidFormattedCount_WhenGettingNames_ThenItsNumericValueIsPassedToTheService(
            string countValue,
            int expectedCount)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, expectedCount))
                .Returns(ExpectedNames);
            string query = $"schema={Schema}&count={Uri.EscapeDataString(countValue)}";

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(query);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames(Schema, expectedCount),
                Times.Once);
        }

        [Test]
        public async Task GivenAMissingSchema_WhenGettingNames_ThenANullSchemaIsPassedToTheService()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(null!, 4))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync("count=4");

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(service => service.GetNames(null!, 4), Times.Once);
        }

        [Test]
        public async Task GivenAnEmptySchema_WhenGettingNames_ThenANullSchemaIsPassedToTheService()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(null!, 4))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync("schema=&count=4");

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(service => service.GetNames(null!, 4), Times.Once);
        }

        [TestCase(" ")]
        [TestCase("   ")]
        [TestCase("\t")]
        [TestCase("\r\n")]
        public async Task GivenAWhitespaceSchema_WhenGettingNames_ThenANullSchemaIsPassedToTheService(
            string schema)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(null!, 4))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(BuildQuery(schema, 4));

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(service => service.GetNames(null!, 4), Times.Once);
        }

        [Test]
        public async Task GivenNoQueryParameters_WhenGettingNames_ThenTheDefaultValuesArePassedToTheService()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(null!, 1))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(string.Empty);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(service => service.GetNames(null!, 1), Times.Once);
        }

        [Test]
        public async Task GivenRepeatedSchemaValues_WhenGettingNames_ThenTheFirstValueIsPassedToTheService()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames("Astora", 4))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(
                "schema=Astora&schema=Nucilandia&count=4");

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(service => service.GetNames("Astora", 4), Times.Once);
        }

        [Test]
        public async Task GivenRepeatedCountValues_WhenGettingNames_ThenTheFirstValueIsPassedToTheService()
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames(Schema, 4))
                .Returns(ExpectedNames);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(
                $"schema={Schema}&count=4&count=8");

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ExpectedNames);
            NameGeneratorServiceMock.Verify(service => service.GetNames(Schema, 4), Times.Once);
        }
    }
}