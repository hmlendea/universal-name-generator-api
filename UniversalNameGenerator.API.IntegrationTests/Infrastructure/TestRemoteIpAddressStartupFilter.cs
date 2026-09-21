using System;
using System.Net;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace UniversalNameGenerator.API.IntegrationTests.Infrastructure
{
    internal sealed class TestRemoteIpAddressStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => applicationBuilder =>
            {
                applicationBuilder.Use(async (httpContext, nextMiddleware) =>
                {
                    httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

                    await nextMiddleware();
                });

                next(applicationBuilder);
            };
    }
}