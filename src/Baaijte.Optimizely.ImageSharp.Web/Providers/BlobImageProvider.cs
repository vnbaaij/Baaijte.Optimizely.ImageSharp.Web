using Baaijte.Optimizely.ImageSharp.Web.Resolvers;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;
using System;
using System.Threading.Tasks;

namespace Baaijte.Optimizely.ImageSharp.Web.Providers
{
    public class BlobImageProvider : IImageProvider
    {

        /// <summary>
        /// A match function used by the resolver to identify itself as the correct resolver to use.
        /// </summary>
        private Func<HttpContext, bool> _match;

        /// <summary>
        /// Contains various format helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities _formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobImageProvider"/> class.
        /// </summary>
        /// <param name="environment">The environment used by this middleware.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public BlobImageProvider(IWebHostEnvironment environment, FormatUtilities formatUtilities)
        {
            _formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.CommandOnly;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match
        {
            get => _match ?? IsMatch;
            set => _match = value;
        }

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context) => _formatUtilities.TryGetExtensionFromUri(context.Request.GetDisplayUrl(), out _);

        /// <inheritdoc/>
        public Task<IImageResolver> GetAsync(HttpContext context)
        {
            string url = context.Request.Path.Value;

            MediaData media = UrlResolver.Current.Route(new UrlBuilder(url)) as MediaData;

            if (media != null && media.BinaryData != null)
            {
                return Task.FromResult<IImageResolver>(new BlobImageResolver(media.BinaryData));
            }
            return Task.FromResult<IImageResolver>(null);
        }

        private bool IsMatch(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/contentassets", StringComparison.OrdinalIgnoreCase))
                return true;

            if (context.Request.Path.StartsWithSegments("/globalassets", StringComparison.OrdinalIgnoreCase))
                return true;

            if (context.Request.Path.StartsWithSegments("/siteassets", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
