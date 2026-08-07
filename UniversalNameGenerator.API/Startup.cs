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
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration => configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy
                        .WithOrigins(
                            "http://localhost:5000",
                            "https://localhost:5001",
                            "http://localhost:7000",
                            "https://localhost:7001",
                            "http://localhost:8080",
                            "http://localhost:8081")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            services
                .AddConfigurations(Configuration)
                .AddNuciApiScannerProtection()
                .AddCustomServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseNuciApiExceptionHandling();
            app.UseNuciApiScannerProtection();
            app.UseNuciApiRequestLogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
