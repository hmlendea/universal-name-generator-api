using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Authentication;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Moq;

using NUnit.Framework;

using NuciAPI.Responses;

using UniversalNameGenerator.API.IntegrationTests.Assertions;

namespace UniversalNameGenerator.API.IntegrationTests.Controllers
{
    [TestFixture]
    public sealed class NamesControllerExceptionHandlingTests : NamesControllerTestBase
    {
        private static string Query => "schema=astora-settlements&count=4";

        private static HttpStatusCode ClientClosedRequestStatusCode => (HttpStatusCode)499;

        private static IEnumerable<TestCaseData> ServiceExceptionCases
        {
            get
            {
                yield return BuildBadRequestCase(
                    new ArgumentException("The requested count is invalid."),
                    "GivenAnArgumentException_WhenGettingNames_ThenABadRequestResponseIsReturned");
                yield return BuildBadRequestCase(
                    new ArgumentNullException("schema"),
                    "GivenAnArgumentNullException_WhenGettingNames_ThenABadRequestResponseIsReturned");
                yield return BuildBadRequestCase(
                    new FormatException("The schema command is malformed."),
                    "GivenAFormatException_WhenGettingNames_ThenABadRequestResponseIsReturned");
                yield return BuildBadRequestCase(
                    new ValidationException("The schema is not valid."),
                    "GivenAValidationException_WhenGettingNames_ThenABadRequestResponseIsReturned");
                yield return BuildBadRequestCase(
                    new BadHttpRequestException("The request body is invalid."),
                    "GivenABadHttpRequestException_WhenGettingNames_ThenABadRequestResponseIsReturned");
                yield return BuildStandardErrorCase(
                    new SecurityException("Access is prohibited."),
                    HttpStatusCode.Forbidden,
                    NuciApiResponseMessages.ErrorMessages.Unauthorised,
                    NuciApiResponseCodes.ErrorCodes.Unauthorised,
                    "GivenASecurityException_WhenGettingNames_ThenAForbiddenResponseIsReturned");
                yield return BuildStandardErrorCase(
                    new UnauthorizedAccessException("Access is prohibited."),
                    HttpStatusCode.Forbidden,
                    NuciApiResponseMessages.ErrorMessages.Unauthorised,
                    NuciApiResponseCodes.ErrorCodes.Unauthorised,
                    "GivenAnUnauthorizedAccessException_WhenGettingNames_ThenAForbiddenResponseIsReturned");
                yield return BuildStandardErrorCase(
                    new AuthenticationException("Authentication failed."),
                    HttpStatusCode.Unauthorized,
                    NuciApiResponseMessages.ErrorMessages.AuthenticationFailure,
                    NuciApiResponseCodes.ErrorCodes.AuthenticationFailure,
                    "GivenAnAuthenticationException_WhenGettingNames_ThenAnUnauthorizedResponseIsReturned");
                yield return BuildStandardErrorCase(
                    new KeyNotFoundException("The schema was not discovered."),
                    HttpStatusCode.NotFound,
                    NuciApiResponseMessages.ErrorMessages.NotFound,
                    NuciApiResponseCodes.ErrorCodes.NotFound,
                    "GivenAKeyNotFoundException_WhenGettingNames_ThenANotFoundResponseIsReturned");
                yield return BuildStandardErrorCase(
                    new HttpRequestException("A dependency request failed."),
                    HttpStatusCode.ServiceUnavailable,
                    NuciApiResponseMessages.ErrorMessages.ServiceDependencyUnavailable,
                    NuciApiResponseCodes.ErrorCodes.ServiceDependencyUnavailable,
                    "GivenAnHttpRequestException_WhenGettingNames_ThenAServiceUnavailableResponseIsReturned");
                yield return BuildStandardErrorCase(
                    new TaskCanceledException("A dependency request was cancelled."),
                    HttpStatusCode.ServiceUnavailable,
                    NuciApiResponseMessages.ErrorMessages.ServiceDependencyUnavailable,
                    NuciApiResponseCodes.ErrorCodes.ServiceDependencyUnavailable,
                    "GivenATaskCanceledException_WhenGettingNames_ThenAServiceUnavailableResponseIsReturned");
                yield return BuildStandardErrorCase(
                    new TimeoutException("A dependency request timed out."),
                    HttpStatusCode.ServiceUnavailable,
                    NuciApiResponseMessages.ErrorMessages.ServiceDependencyUnavailable,
                    NuciApiResponseCodes.ErrorCodes.ServiceDependencyUnavailable,
                    "GivenATimeoutException_WhenGettingNames_ThenAServiceUnavailableResponseIsReturned");
                yield return BuildStandardErrorCase(
                    new OperationCanceledException("The client disconnected."),
                    ClientClosedRequestStatusCode,
                    NuciApiResponseMessages.ErrorMessages.ClientClosedTheRequest,
                    NuciApiResponseCodes.ErrorCodes.ClientClosedTheRequest,
                    "GivenAnOperationCanceledException_WhenGettingNames_ThenAClientClosedResponseIsReturned");
                yield return BuildNotImplementedCase(
                    new NotImplementedException("The generator is not implemented."),
                    "GivenANotImplementedException_WhenGettingNames_ThenANotImplementedResponseIsReturned");
                yield return BuildInternalServerErrorCase(
                    new InvalidOperationException("The operation is invalid."),
                    "GivenAnInvalidOperationException_WhenGettingNames_ThenAnInternalServerErrorIsReturned");
                yield return BuildInternalServerErrorCase(
                    new NotSupportedException("The command is unsupported."),
                    "GivenANotSupportedException_WhenGettingNames_ThenAnInternalServerErrorIsReturned");
                yield return BuildInternalServerErrorCase(
                    new FileNotFoundException("The word list was not discovered."),
                    "GivenAFileNotFoundException_WhenGettingNames_ThenAnInternalServerErrorIsReturned");
                yield return BuildInternalServerErrorCase(
                    new IOException("The data store could not be read."),
                    "GivenAnIOException_WhenGettingNames_ThenAnInternalServerErrorIsReturned");
                yield return BuildInternalServerErrorCase(
                    new NullReferenceException("The generated value was null."),
                    "GivenANullReferenceException_WhenGettingNames_ThenAnInternalServerErrorIsReturned");
                yield return BuildInternalServerErrorCase(
                    new OverflowException("The generated value overflowed."),
                    "GivenAnOverflowException_WhenGettingNames_ThenAnInternalServerErrorIsReturned");
            }
        }

        [TestCaseSource(nameof(ServiceExceptionCases))]
        public async Task GivenAServiceException_WhenGettingNames_ThenTheExceptionIsMappedToTheExpectedResponse(
            Exception serviceException,
            HttpStatusCode expectedStatusCode,
            string expectedMessage,
            string expectedCode)
        {
            NameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Throws(serviceException);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(Query);

            await NuciApiResponseAssertions.AssertErrorResponseAsync(
                response,
                expectedStatusCode,
                expectedMessage,
                expectedCode);
            NameGeneratorServiceMock.Verify(
                service => service.GetNames("astora-settlements", 4),
                Times.Once);
        }

        [Test]
        public async Task GivenADeferredEnumerationException_WhenSerialisingNames_ThenAnInternalServerErrorIsReturned()
        {
            IEnumerable<string> names = Enumerable.Range(1, 4).Select<int, string>(
                number => throw new InvalidOperationException($"Name {number} could not be generated."));
            NameGeneratorServiceMock
                .Setup(service => service.GetNames("astora-settlements", 4))
                .Returns(names);

            using HttpResponseMessage response = await SendAuthorisedGetRequestAsync(Query);

            await NuciApiResponseAssertions.AssertErrorResponseAsync(
                response,
                HttpStatusCode.InternalServerError,
                NuciApiResponseMessages.ErrorMessages.InternalServerError,
                NuciApiResponseCodes.ErrorCodes.InternalServerError);
        }

        private static TestCaseData BuildBadRequestCase(Exception serviceException, string testName)
            => new TestCaseData(
                serviceException,
                HttpStatusCode.BadRequest,
                serviceException.Message,
                NuciApiResponseCodes.ErrorCodes.BadRequest)
                .SetName(testName);

        private static TestCaseData BuildNotImplementedCase(Exception serviceException, string testName)
            => new TestCaseData(
                serviceException,
                HttpStatusCode.NotImplemented,
                serviceException.Message,
                NuciApiResponseCodes.ErrorCodes.NotImplemented)
                .SetName(testName);

        private static TestCaseData BuildInternalServerErrorCase(Exception serviceException, string testName)
            => BuildStandardErrorCase(
                serviceException,
                HttpStatusCode.InternalServerError,
                NuciApiResponseMessages.ErrorMessages.InternalServerError,
                NuciApiResponseCodes.ErrorCodes.InternalServerError,
                testName);

        private static TestCaseData BuildStandardErrorCase(
            Exception serviceException,
            HttpStatusCode expectedStatusCode,
            string expectedMessage,
            string expectedCode,
            string testName)
            => new TestCaseData(
                serviceException,
                expectedStatusCode,
                expectedMessage,
                expectedCode)
                .SetName(testName);
    }
}