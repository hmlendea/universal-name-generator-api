using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using NuciAPI.Responses;

namespace UniversalNameGenerator.API.IntegrationTests.Assertions
{
    internal static class NuciApiResponseAssertions
    {
        private static string JsonMediaType => "application/json";

        private static string ProblemJsonMediaType => "application/problem+json";

        private static IEnumerable<string> SuccessPropertyNames =>
        [
            "names",
            "success",
            "message",
            "code",
            "hmac"
        ];

        private static IEnumerable<string> ErrorPropertyNames =>
        [
            "success",
            "message",
            "code",
            "hmac"
        ];

        public static async Task AssertSuccessfulNamesResponseAsync(
            HttpResponseMessage response,
            IEnumerable<string>? expectedNames)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            using JsonDocument responseDocument = JsonDocument.Parse(responseContent);
            JsonElement responseRoot = responseDocument.RootElement;
            JsonElement namesElement = responseRoot.GetProperty("names");

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), responseContent);
                Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo(JsonMediaType));
                Assert.That(
                    responseRoot.EnumerateObject().Select(property => property.Name),
                    Is.EquivalentTo(SuccessPropertyNames));
                Assert.That(responseRoot.GetProperty("success").GetBoolean());
                Assert.That(
                    responseRoot.GetProperty("message").GetString(),
                    Is.EqualTo(NuciApiResponseMessages.SuccessMessages.Default));
                Assert.That(
                    responseRoot.GetProperty("code").GetString(),
                    Is.EqualTo(NuciApiResponseCodes.SuccessCodes.Default));
                Assert.That(responseRoot.GetProperty("hmac").ValueKind, Is.EqualTo(JsonValueKind.Null));
                AssertNames(namesElement, expectedNames);
            });
        }

        public static async Task AssertErrorResponseAsync(
            HttpResponseMessage response,
            HttpStatusCode expectedStatusCode,
            string expectedMessage,
            string expectedCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            using JsonDocument responseDocument = JsonDocument.Parse(responseContent);
            JsonElement responseRoot = responseDocument.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode), responseContent);
                Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo(JsonMediaType));
                Assert.That(
                    responseRoot.EnumerateObject().Select(property => property.Name),
                    Is.EquivalentTo(ErrorPropertyNames));
                Assert.That(responseRoot.GetProperty("success").GetBoolean(), Is.False);
                Assert.That(responseRoot.GetProperty("message").GetString(), Is.EqualTo(expectedMessage));
                Assert.That(responseRoot.GetProperty("code").GetString(), Is.EqualTo(expectedCode));
                Assert.That(responseRoot.GetProperty("hmac").ValueKind, Is.EqualTo(JsonValueKind.Null));
            });
        }

        public static async Task AssertValidationErrorResponseAsync(
            HttpResponseMessage response,
            string expectedFieldName)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            using JsonDocument responseDocument = JsonDocument.Parse(responseContent);
            JsonElement responseRoot = responseDocument.RootElement;
            JsonElement errorsElement = responseRoot.GetProperty("errors");

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), responseContent);
                Assert.That(
                    response.Content.Headers.ContentType?.MediaType,
                    Is.EqualTo(ProblemJsonMediaType));
                Assert.That(responseRoot.GetProperty("status").GetInt32(), Is.EqualTo((int)HttpStatusCode.BadRequest));
                Assert.That(errorsElement.TryGetProperty(expectedFieldName, out JsonElement fieldErrors));
                Assert.That(fieldErrors.GetArrayLength(), Is.GreaterThan(0));
            });
        }

        private static void AssertNames(JsonElement namesElement, IEnumerable<string>? expectedNames)
        {
            if (expectedNames is null)
            {
                Assert.That(namesElement.ValueKind, Is.EqualTo(JsonValueKind.Null));

                return;
            }

            string?[] names = [.. namesElement.EnumerateArray().Select(name => name.GetString())];

            Assert.That(names, Is.EqualTo(expectedNames));
        }
    }
}