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
        public static void AddBaaijteOptimizelyImageSharp(this IServiceCollection services)
        {
            services.AddOptions<ImageRequestSigningOptions>()
                    .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Salt), "Image request signing requires a salt when enabled.");

            services.AddSingleton<ImageRequestHashService>();

            services.AddImageSharp()
                    .ClearProviders()
                    .AddProvider<BlobImageProvider>()
                    .AddProvider<PhysicalFileSystemProvider>()
                    .SetCache<BlobImageCache>();
        }

        public static void UseBaaijteOptimizelyImageSharp(this IApplicationBuilder app)
        {
            app.UseWhen(ManagedImageRequestMatcher.IsManagedImageRequest, branch =>
            {
                branch.Use(async (context, next) =>
                {
                    var imageRequestHashService = context.RequestServices.GetRequiredService<ImageRequestHashService>();

                    if (!imageRequestHashService.IsAuthorized(context.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }

                    await next();
                });
            });

            app.UseImageSharp();
        }
    }
}
