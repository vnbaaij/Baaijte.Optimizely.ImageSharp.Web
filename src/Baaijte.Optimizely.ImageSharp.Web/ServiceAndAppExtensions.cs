using Baaijte.Optimizely.ImageSharp.Web.Caching;
using Baaijte.Optimizely.ImageSharp.Web.Providers;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers;

namespace Baaijte.Optimizely.ImageSharp.Web
{
    public static class ServiceAndAppExtensions
    {
        public static void AddBaaijteOptimizelyImageSharp(this IServiceCollection services)
        {
            services.AddImageSharp()
                    .ClearProviders()
                    .AddProvider<BlobImageProvider>()
                    .AddProvider<PhysicalFileSystemProvider>()
                    .SetCache<BlobImageCache>();
        }

        public static void UseBaaijteOptimizelyImageSharp(this IApplicationBuilder app)
        {
            app.UseImageSharp();
        }
    }
}
