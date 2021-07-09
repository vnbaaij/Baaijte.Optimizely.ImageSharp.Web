using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.FileProviders;

using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Baaijte.Optimizely.ImageSharp.Web.Caching
{
    /// <summary>
    /// Provides means to manage image buffers within the <see cref="BlobImageCache"/>.
    /// </summary>
    public class BlobImageCacheResolver : IImageCacheResolver
    {
        private readonly IFileInfo fileInfo;
        private readonly ImageCacheMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobImageCacheResolver"/> class.
        /// </summary>
        /// <param name="fileInfo">The input file info.</param>
        /// <param name="metadata">The image metadata associated with this file.</param>
        public BlobImageCacheResolver(IFileInfo fileInfo, in ImageCacheMetadata metadata)
        {
            this.fileInfo = fileInfo;
            this.metadata = metadata;
        }

        public Task<ImageCacheMetadata> GetMetaDataAsync() => Task.FromResult(this.metadata);

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(this.fileInfo.CreateReadStream());
    }
}
