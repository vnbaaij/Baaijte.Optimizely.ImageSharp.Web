using System.IO;
using System.Threading.Tasks;

using EPiServer.Framework.Blobs;

using Microsoft.Extensions.FileProviders;

using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Baaijte.Optimizely.ImageSharp.Web.Resolvers
{
    public class BlobImageResolver : IImageResolver
    {
        private readonly Blob blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobImageResolver"/> class.
        /// </summary>
        /// <param name="blob">The image blob.</param>

        public BlobImageResolver(Blob blob)
        {
            this.blob = blob;
        }

        /// <inheritdoc/>
        public async Task<ImageMetadata> GetMetaDataAsync()
        {
            IFileInfo fileInfo = await blob.AsFileInfoAsync();

            return new(fileInfo.LastModified.UtcDateTime, fileInfo.Length);
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(blob.OpenRead());

    }
}
