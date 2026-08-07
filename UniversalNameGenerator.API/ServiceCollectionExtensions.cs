using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciLog;
using NuciLog.Core;

using UniversalNameGenerator.API.Configuration;
using UniversalNameGenerator.API.Service;

namespace UniversalNameGenerator.API
{
    public static class ServiceCollectionExtensions
    {
        private static string DataStoreSettingsSectionName => "DataStoreSettings";

        private static string SecuritySettingsSectionName => "SecuritySettings";

        public static IServiceCollection AddConfigurations(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            DataStoreSettings dataStoreSettings = new();
            SecuritySettings securitySettings = new();

            configuration.Bind(DataStoreSettingsSectionName, dataStoreSettings);
            configuration.Bind(SecuritySettingsSectionName, securitySettings);

            return services
                .AddSingleton(dataStoreSettings)
                .AddSingleton(securitySettings)
                .AddNuciLoggerSettings(configuration);
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services)
            => services
                .AddScoped<INameGeneratorService, NameGeneratorService>()
                .AddScoped<ILogger, NuciLogger>();
    }
}
