using System;
using System.IO;
using System.Threading.Tasks;
using EPiServer.Core;
using Microsoft.Extensions.FileProviders;

using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Baaijte.Optimizely.ImageSharp.Web.Resolvers
{
    public class BlobImageResolver : IImageResolver
    {
        private readonly MediaData media;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobImageResolver"/> class.
        /// </summary>
        /// <param name="media">The image.</param>

        public BlobImageResolver(MediaData media)
        {
            this.media = media;
        }

        /// <inheritdoc/>
        public async Task<ImageMetadata> GetMetaDataAsync()
        {
            DateTimeOffset lastModified = media.Saved;
            IFileInfo fileInfo = await media.BinaryData.AsFileInfoAsync(lastModified);

            return new(lastModified.UtcDateTime, fileInfo.Length);
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => Task.FromResult(media.BinaryData.OpenRead());

    }
}
