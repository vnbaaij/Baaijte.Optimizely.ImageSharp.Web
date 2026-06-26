using System;

using Baaijte.Optimizely.ImageSharp.Web.Caching;
using Baaijte.Optimizely.ImageSharp.Web.Providers;
using Baaijte.Optimizely.ImageSharp.Web.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers;

namespace Baaijte.Optimizely.ImageSharp.Web
{
    public static class ServiceAndAppExtensions
    {
        public static void AddImageRequestSigning(this IServiceCollection services, ImageRequestSigningOptions options)
        {
            ArgumentNullException.ThrowIfNull(services);

            ArgumentNullException.ThrowIfNull(options);

            services.AddOptions<ImageRequestSigningOptions>()
                    .Configure(configure =>
                    {
                        configure.Enabled = options.Enabled;
                        configure.Salt = options.Salt;
                    })
                    .Validate(option => option.Enabled == false || !string.IsNullOrWhiteSpace(option.Salt), "Image request signing requires a salt when enabled.");

            services.AddSingleton<ImageRequestHashService>();
        }

        public static void AddBaaijteOptimizelyImageSharp(this IServiceCollection services)
        {
            //services.AddImageRequestSigning(new ImageRequestSigningOptions());

            services.AddImageSharp()
                    .ClearProviders()
                    .AddProvider<BlobImageProvider>()
                    .AddProvider<PhysicalFileSystemProvider>()
                    .SetCache<BlobImageCache>();
        }

        public static void UseBaaijteOptimizelyImageSharp(this IApplicationBuilder app)
        {
            var serviceProviderIsService = app.ApplicationServices.GetService<IServiceProviderIsService>();
            
            var hasImageRequestSigning = 
                serviceProviderIsService?.IsService(typeof(ImageRequestHashService)) == true;

            if (hasImageRequestSigning)
            {
                app.UseWhen(ManagedImageRequestMatcher.IsManagedImageRequest, branch =>
                {
                    branch.Use(async (context, next) =>
                    {
                        var imageRequestHashService =
                            context.RequestServices.GetRequiredService<ImageRequestHashService>();

                        if (!imageRequestHashService.IsAuthorized(context.Request))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return;
                        }

                        await next();
                    });
                });
            }

            app.UseImageSharp();
        }
    }
}
