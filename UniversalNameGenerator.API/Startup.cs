using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NuciAPI.Middleware.ExceptionHandling;
using NuciAPI.Middleware.Logging;
using NuciAPI.Middleware.Security;

using UniversalNameGenerator.API.Configuration;

namespace UniversalNameGenerator.API
{
    public sealed class Startup(IConfiguration configuration) : IStartup
    {
        private static string[] AllowedCorsOrigins =>
        [
            "http://localhost:5000",
            "https://localhost:5001",
            "http://localhost:7000",
            "https://localhost:7001",
            "http://localhost:8080",
            "http://localhost:8081"
        ];

        public IConfiguration Configuration => configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy
                        .WithOrigins(AllowedCorsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            services
                .AddConfigurations(Configuration)
                .AddNuciApiScannerProtection()
                .AddCustomServices();
        }

        public void Configure(IApplicationBuilder applicationBuilder, IWebHostEnvironment hostingEnvironment)
        {
            applicationBuilder.UseNuciApiExceptionHandling();
            applicationBuilder.UseNuciApiScannerProtection();
            applicationBuilder.UseNuciApiRequestLogging();

            if (hostingEnvironment.IsDevelopment())
            {
                applicationBuilder.UseDeveloperExceptionPage();
            }

            applicationBuilder.UseHttpsRedirection();
            applicationBuilder.UseCors();
            applicationBuilder.UseDefaultFiles();
            applicationBuilder.UseStaticFiles();
            applicationBuilder.UseRouting();
            applicationBuilder.UseAuthorization();

            applicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
