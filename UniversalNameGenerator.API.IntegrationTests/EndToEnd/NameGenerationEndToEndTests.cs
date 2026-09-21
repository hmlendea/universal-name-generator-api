using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using NuciAPI.Responses;

using UniversalNameGenerator.API.IntegrationTests.Assertions;
using UniversalNameGenerator.API.IntegrationTests.Infrastructure;

namespace UniversalNameGenerator.API.IntegrationTests.EndToEnd
{
    [TestFixture]
    public sealed class NameGenerationEndToEndTests
    {
        private static IEnumerable<string> Cities =>
        [
            "Astora",
            "Cluj-Napoca",
            "Cornova",
            "Cratesia",
            "Çupișan",
            "Dezmir",
            "Enada",
            "Florești",
            "Flusseland",
            "Frigonița",
            "Hokazuro",
            "Horidava",
            "Izmir",
            "Newport",
            "Nordavia",
            "Oradea",
            "Solara"
        ];

        private static IEnumerable<string> FilteredCities =>
        [
            "Astora",
            "Cluj-Napoca",
            "Cornova",
            "Cratesia",
            "Çupișan",
            "Dezmir",
            "Enada",
            "Florești",
            "Flusseland",
            "Frigonița",
            "Hokazuro",
            "Horidava",
            "Izmir",
            "Newport",
            "Nordavia",
            "Solara"
        ];

        private static IEnumerable<string> FirstNames =>
        [
            "Ilarion",
            "Ionuț",
            "Oscar",
            "Robert",
            "Tibi",
            "Vasile",
            "Bobert",
            "Solaire"
        ];

        private static IEnumerable<string> LastNames =>
        [
            "Ciupitu",
            "Nucaru",
            "Nucescu",
            "Pintilie",
            "Blitz",
            "Karr"
        ];

        private static IEnumerable<string> FullNames =>
            from firstName in FirstNames
            from lastName in LastNames
            select $"{firstName} {lastName}";

        private IntegrationGenerationDataStore dataStore = null!;
        private UniversalNameGeneratorApiFactory applicationFactory = null!;
        private HttpClient httpClient = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            dataStore = new IntegrationGenerationDataStore();
            applicationFactory = new UniversalNameGeneratorApiFactory(dataStore.Settings);
            httpClient = applicationFactory.CreateClient();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            httpClient.Dispose();
            applicationFactory.Dispose();
            dataStore.Dispose();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(32)]
        [TestCase(42)]
        [TestCase(64)]
        [TestCase(96)]
        [TestCase(128)]
        public async Task GivenADeterministicSchema_WhenGettingNames_ThenTheProductionServiceGeneratesEveryName(
            int count)
        {
            IEnumerable<string> expectedNames = Enumerable.Repeat("Ab", count);

            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.DeterministicSchemaId,
                count);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, expectedNames);
        }

        [Test]
        public async Task GivenNoCount_WhenGettingNames_ThenTheProductionServiceGeneratesOneName()
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                $"/Names?schema={IntegrationGenerationDataStore.DeterministicSchemaId}");

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, ["Ab"]);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(32)]
        public async Task GivenALowerCaseSchema_WhenGettingNames_ThenEveryNameIsLowerCase(int count)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.LowerCaseSchemaId,
                count);
            string[] names = await ReadNamesAsync(response);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
            Assert.Multiple(() =>
            {
                Assert.That(names, Has.Length.EqualTo(count));
                Assert.That(names.All(name => name.Length == 4));
                Assert.That(names.All(name => string.Equals(name, name.ToLowerInvariant(), StringComparison.Ordinal)));
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(32)]
        public async Task GivenAnUpperCaseSchema_WhenGettingNames_ThenEveryNameIsUpperCase(int count)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.UpperCaseSchemaId,
                count);
            string[] names = await ReadNamesAsync(response);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
            Assert.Multiple(() =>
            {
                Assert.That(names, Has.Length.EqualTo(count));
                Assert.That(names.All(name => name.Length == 4));
                Assert.That(names.All(name => string.Equals(name, name.ToUpperInvariant(), StringComparison.Ordinal)));
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(32)]
        public async Task GivenATitleCaseSchema_WhenGettingNames_ThenEveryNameIsTitleCase(int count)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.TitleCaseSchemaId,
                count);
            string[] names = await ReadNamesAsync(response);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
            Assert.Multiple(() =>
            {
                Assert.That(names, Has.Length.EqualTo(count));
                Assert.That(names.All(name => name.Length == 4));
                Assert.That(names.All(name => char.IsUpper(name[0])));
                Assert.That(names.All(name => name.Skip(1).All(character => char.IsLower(character))));
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(8)]
        [TestCase(16)]
        public async Task GivenARandomSelectorSchema_WhenGettingNames_ThenAvailableCitiesAreSelected(int count)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.SelectorSchemaId,
                count);
            string[] names = await ReadNamesAsync(response);
            int expectedCount = GetBoundedCount(count, Cities.Count());

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
            Assert.Multiple(() =>
            {
                Assert.That(names, Has.Length.EqualTo(expectedCount));
                Assert.That(names, Is.Unique);
                Assert.That(names, Is.SubsetOf(Cities));
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        public async Task GivenAFilteredSelectorSchema_WhenGettingNames_ThenExcludedCitiesAreNeverReturned(
            int count)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.FilteredSelectorSchemaId,
                count);
            string[] names = await ReadNamesAsync(response);
            int expectedCount = GetBoundedCount(count, FilteredCities.Count());

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
            Assert.Multiple(() =>
            {
                Assert.That(names, Has.Length.EqualTo(expectedCount));
                Assert.That(names, Is.Unique);
                Assert.That(names, Is.SubsetOf(FilteredCities));
                Assert.That(names, Does.Not.Contain("Oradea"));
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(16)]
        [TestCase(32)]
        public async Task GivenARandomiserSchema_WhenGettingNames_ThenAvailableCombinationsAreGenerated(
            int count)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.RandomiserSchemaId,
                count);
            string[] names = await ReadNamesAsync(response);
            int expectedCount = GetBoundedCount(count, FullNames.Count());

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
            Assert.Multiple(() =>
            {
                Assert.That(names, Has.Length.EqualTo(expectedCount));
                Assert.That(names, Is.Unique);
                Assert.That(names, Is.SubsetOf(FullNames));
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public async Task GivenAMarkovSchema_WhenGettingNames_ThenNamesWithinTheConfiguredLengthsAreGenerated(
            int count)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.MarkovSchemaId,
                count);
            string[] names = await ReadNamesAsync(response);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, names);
            Assert.Multiple(() =>
            {
                Assert.That(names, Has.Length.EqualTo(count));
                Assert.That(names.All(name => name.Length >= 20));
                Assert.That(names.All(name => name.Length <= 28));
                Assert.That(names.All(name => char.IsUpper(name[0])));
            });
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [TestCase(128)]
        public async Task GivenASchemaWithoutGenerators_WhenGettingNames_ThenNoNamesAreReturned(int count)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.LiteralSchemaId,
                count);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, []);
        }

        [TestCase("schema-does-not-exist")]
        [TestCase("Astora-settlements")]
        [TestCase("ASTORA-SETTLEMENTS")]
        [TestCase("unknown")]
        [TestCase("1234567890")]
        [TestCase(" ")]
        [TestCase("   ")]
        public async Task GivenANonExistentSchema_WhenGettingNames_ThenNotFoundIsReturned(string schema)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(schema, 4);

            await NuciApiResponseAssertions.AssertErrorResponseAsync(
                response,
                HttpStatusCode.NotFound,
                NuciApiResponseMessages.ErrorMessages.NotFound,
                NuciApiResponseCodes.ErrorCodes.NotFound);
        }

        [Test]
        public async Task GivenAMissingSchema_WhenGettingNames_ThenNotFoundIsReturned()
        {
            using HttpResponseMessage response = await SendGetRequestAsync("/Names?count=4");

            await NuciApiResponseAssertions.AssertErrorResponseAsync(
                response,
                HttpStatusCode.NotFound,
                NuciApiResponseMessages.ErrorMessages.NotFound,
                NuciApiResponseCodes.ErrorCodes.NotFound);
        }

        [Test]
        public async Task GivenAnUnsupportedGeneratorCommand_WhenGettingNames_ThenAnInternalServerErrorIsReturned()
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.UnsupportedSchemaId,
                4);

            await AssertInternalServerErrorAsync(response);
        }

        [Test]
        public async Task GivenAMissingWordList_WhenGettingNames_ThenAnInternalServerErrorIsReturned()
        {
            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.MissingWordListSchemaId,
                4);

            await AssertInternalServerErrorAsync(response);
        }

        [TestCase("invalid-random-integer", "The value 'four' for 'minimumLength' in command 'random,so|la,four,8' is not a valid integer.")]
        [TestCase("invalid-random-range", "Minimum length '16' cannot exceed maximum length '4'.")]
        [TestCase("invalid-random", "Command 'random,so' does not include sufficient values.")]
        [TestCase("invalid-randomiser", "Randomiser command does not include sufficient values.")]
        [TestCase("invalid-selector", "Random selector command does not include sufficient values.")]
        [TestCase("invalid-markov", "Markov command does not include sufficient values.")]
        public async Task GivenAMalformedGeneratorCommand_WhenGettingNames_ThenABadRequestIsReturned(
            string schema,
            string expectedMessage)
        {
            using HttpResponseMessage response = await SendGetRequestAsync(schema, 4);

            await NuciApiResponseAssertions.AssertErrorResponseAsync(
                response,
                HttpStatusCode.BadRequest,
                expectedMessage,
                NuciApiResponseCodes.ErrorCodes.BadRequest);
        }

        [Test]
        public async Task GivenALargeRequest_WhenGettingNames_ThenEveryNameIsSerialised()
        {
            int count = 512;
            IEnumerable<string> expectedNames = Enumerable.Repeat("Ab", count);

            using HttpResponseMessage response = await SendGetRequestAsync(
                IntegrationGenerationDataStore.DeterministicSchemaId,
                count);

            await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(response, expectedNames);
        }

        [Test]
        public async Task GivenConcurrentRequests_WhenGettingNames_ThenEveryResponseIsComplete()
        {
            IEnumerable<Task<HttpResponseMessage>> responseTasks = Enumerable
                .Range(1, 16)
                .Select(count => SendGetRequestAsync(
                    IntegrationGenerationDataStore.DeterministicSchemaId,
                    count));

            HttpResponseMessage[] responses = await Task.WhenAll(responseTasks);

            try
            {
                for (int responseIndex = 0; responseIndex < responses.Length; responseIndex += 1)
                {
                    int expectedCount = responseIndex + 1;
                    IEnumerable<string> expectedNames = Enumerable.Repeat("Ab", expectedCount);

                    await NuciApiResponseAssertions.AssertSuccessfulNamesResponseAsync(
                        responses[responseIndex],
                        expectedNames);
                }
            }
            finally
            {
                foreach (HttpResponseMessage response in responses)
                {
                    response.Dispose();
                }
            }
        }

        private async Task<HttpResponseMessage> SendGetRequestAsync(string schema, int count)
            => await SendGetRequestAsync($"/Names?schema={schema}&count={count}");

        private async Task<HttpResponseMessage> SendGetRequestAsync(string requestUri)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                $"Bearer {UniversalNameGeneratorApiFactory.ApiKey}");

            return await httpClient.SendAsync(request);
        }

        private static async Task<string[]> ReadNamesAsync(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            using JsonDocument responseDocument = JsonDocument.Parse(responseContent);

            return
            [
                .. responseDocument.RootElement
                    .GetProperty("names")
                    .EnumerateArray()
                    .Select(GetRequiredString)
            ];
        }

        private static string GetRequiredString(JsonElement element)
        {
            string? value = element.GetString();

            if (value is null)
            {
                throw new InvalidOperationException("A generated name was null.");
            }

            return value;
        }

        private static int GetBoundedCount(int requestedCount, int availableCount)
        {
            int boundedCount = requestedCount;

            if (boundedCount > availableCount)
            {
                boundedCount = availableCount;
            }

            return boundedCount;
        }

        private static Task AssertInternalServerErrorAsync(HttpResponseMessage response)
            => NuciApiResponseAssertions.AssertErrorResponseAsync(
                response,
                HttpStatusCode.InternalServerError,
                NuciApiResponseMessages.ErrorMessages.InternalServerError,
                NuciApiResponseCodes.ErrorCodes.InternalServerError);
    }
}