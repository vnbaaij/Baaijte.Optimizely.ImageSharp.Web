using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Baaijte.Optimizely.ImageSharp.Web.Caching
{
    /// <summary>
    /// Implements an Optimizely blob based cache.
    /// </summary>
    public class BlobImageCache : IImageCache
    {
        /// <summary>
        /// The root path for the cache.
        /// </summary>
        private readonly string cacheRootPath;

        /// <summary>
        /// The length of the filename to use (minus the extension) when storing images in the image cache.
        /// </summary>
        private readonly int cachedNameLength;

        /// <summary>
        /// The file provider abstraction.
        /// </summary>
        //private readonly IFileProvider fileProvider;

        /// <summary>
        /// The cache configuration options.
        /// </summary>
        private readonly BlobImageCacheOptions cacheOptions;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        /// <summary>
        /// Contains various format helper methods based on the current configuration.
        /// </summary>
        private readonly FormatUtilities formatUtilities;

        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobImageCache"/> class.
        /// </summary>
        /// <param name="cacheOptions">The cache configuration options.</param>
        /// <param name="environment">The hosting environment the application is running in.</param>
        /// <param name="options">The middleware configuration options.</param>
        /// <param name="formatUtilities">Contains various format helper methods based on the current configuration.</param>
        public BlobImageCache(IOptions<BlobImageCacheOptions> cacheOptions, IWebHostEnvironment environment, IOptions<ImageSharpMiddlewareOptions> options, FormatUtilities formatUtilities, IHttpContextAccessor httpContextAccessor)
        {
            // Allow configuration of the cache without having to register everything.
            this.cacheOptions = cacheOptions != null ? cacheOptions.Value : new BlobImageCacheOptions();

            this.httpContextAccessor = httpContextAccessor;

            cacheRootPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, this.cacheOptions.CacheFolder));
            //fileProvider = new PhysicalFileProvider(cacheRootPath);
            this.options = options.Value;
            cachedNameLength = (int)this.options.CacheHashLength;
            this.formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public async Task<IImageCacheResolver> GetAsync(string key)
        {
            string container = GetContainer();
            string fileId = $"{Blob.BlobUriScheme}://{Blob.DefaultProvider}/{container}/{cacheOptions.Prefix}{key}";

            IBlobFactory blobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();
            Uri uri = new($"{fileId}.meta");

            Blob blob = blobFactory.GetBlob(uri);

            IFileInfo metaFileInfo = await blob.AsFileInfoAsync();

            if (!metaFileInfo.Exists)
                return null;

            ImageCacheMetadata metadata = await ImageCacheMetadata.ReadAsync(blob.OpenRead());

            uri = new($"{fileId}{ToImageExtension(metadata)}");
            blob = blobFactory.GetBlob(uri);
            IFileInfo fileInfo = await blob.AsFileInfoAsync();

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return null;
            }

            return new BlobImageCacheResolver(fileInfo, metadata);
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
        {

            string name = $"{cacheOptions.Prefix}{key}";
            string imagefile = $"{name}{ToImageExtension(metadata)}";
            string metafile = $"{name}.meta";

            FileBlob blob = CreateBlob(imagefile);
            blob.Write(stream);

            blob = CreateBlob(metafile);
            await metadata.WriteAsync(blob.OpenWrite());
        }

        private FileBlob CreateBlob(string file)
        {
            string container = GetContainer();
            string path = Path.Combine(cacheRootPath, container, file);

            Uri uri = new($"{Blob.BlobUriScheme}://{Blob.DefaultProvider}/{container}/{file}");
            FileBlob blob = new(uri, path);

            return blob;
        }

        private string GetContainer()
        {
            string url = httpContextAccessor.HttpContext.Request.Path.Value;
            MediaData media = UrlResolver.Current.Route(new UrlBuilder(url)) as MediaData;

            string container = media?.BinaryDataContainer?.Segments[1];
            if (container == null)
            {
                // We're working with a static file here
                container = $"_{cacheOptions.Prefix}static";
            }

            return container;
        }

        /// <summary>
        /// Gets the path to the image file based on the supplied root and metadata.
        /// </summary>
        /// <param name="path">The root path.</param>
        /// <param name="metaData">The image metadata.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ToImageExtension(in ImageCacheMetadata metaData)
            => $".{formatUtilities.GetExtensionFromContentType(metaData.ContentType)}";
    }
}
