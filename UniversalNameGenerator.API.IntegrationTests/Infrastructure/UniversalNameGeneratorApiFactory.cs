using System.Collections.Generic;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Moq;

using NuciLog.Core;

using UniversalNameGenerator.API.Configuration;
using UniversalNameGenerator.API.Service;

namespace UniversalNameGenerator.API.IntegrationTests.Infrastructure
{
    public sealed class UniversalNameGeneratorApiFactory : WebApplicationFactory<Startup>
    {
        private readonly DataStoreSettings? dataStoreSettings;

        public static string ApiKey => "NucileRullz!";

        public Mock<INameGeneratorService> NameGeneratorServiceMock { get; } = new(MockBehavior.Strict);

        public UniversalNameGeneratorApiFactory()
        {
        }

        public UniversalNameGeneratorApiFactory(DataStoreSettings dataStoreSettings)
            => this.dataStoreSettings = dataStoreSettings;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Production);

            builder.ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) =>
            {
                IEnumerable<KeyValuePair<string, string?>> settings =
                [
                    new("securitySettings:apiKey", ApiKey),
                    new("nuciLoggerSettings:isFileOutputEnabled", bool.FalseString)
                ];

                configurationBuilder.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ILogger>();
                services.AddSingleton<IStartupFilter, TestRemoteIpAddressStartupFilter>();
                services.AddSingleton<ILogger>(new Mock<ILogger>().Object);

                if (dataStoreSettings is null)
                {
                    services.RemoveAll<INameGeneratorService>();
                    services.AddSingleton<INameGeneratorService>(NameGeneratorServiceMock.Object);
                }
                else
                {
                    services.RemoveAll<DataStoreSettings>();
                    services.AddSingleton(dataStoreSettings);
                }
            });
        }
    }
}