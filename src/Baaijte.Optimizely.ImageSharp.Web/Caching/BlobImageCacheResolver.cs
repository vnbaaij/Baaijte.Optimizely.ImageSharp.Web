using System.IO;

using System.Threading.Tasks;
using EPiServer.Framework.Blobs;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Baaijte.Optimizely.ImageSharp.Web.Caching
{
    /// <summary>
    /// Provides means to manage image buffers within the <see cref="BlobImageCache"/>.
    /// </summary>
    public class BlobImageCacheResolver : IImageCacheResolver
    {
        private readonly Blob blob;
        private readonly ImageCacheMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobImageCacheResolver"/> class.
        /// </summary>
        /// <param name="blob">The blob.</param>
        /// <param name="metadata">The image metadata associated with this file.</param>
        public BlobImageCacheResolver(Blob blob, in ImageCacheMetadata metadata)
        {
            this.blob = blob;
            this.metadata = metadata;
        }

        public Task<ImageCacheMetadata> GetMetaDataAsync() => Task.FromResult(metadata);

        /// <inheritdoc/>
        public async Task<Stream> OpenReadAsync() => await blob.OpenReadAsync();
    }
}
