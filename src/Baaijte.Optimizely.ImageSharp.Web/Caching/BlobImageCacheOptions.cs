namespace Baaijte.Optimizely.ImageSharp.Web.Caching
{
    public class BlobImageCacheOptions
    {
        /// <summary>
        /// Gets or sets the cache folder name.
        /// </summary>
        public string CacheFolder { get; set; } = "App_Data/blobs";

        /// <summary>
        /// Gets or sets the cached filename prefix.
        /// </summary>
        public string Prefix { get; set; } = "is_";
    }
}
