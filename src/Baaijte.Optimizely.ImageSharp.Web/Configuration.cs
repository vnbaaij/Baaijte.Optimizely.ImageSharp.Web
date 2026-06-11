using System;

using Microsoft.AspNetCore.Http;

namespace Baaijte.Optimizely.ImageSharp.Web
{
    internal static class ManagedImageRequestMatcher
    {
        public static bool IsManagedImageRequest(HttpContext context)
        {
            return IsManagedImageRequest(context.Request.Path);
        }

        public static bool IsManagedImageRequest(PathString path)
        {
            if (path.StartsWithSegments("/contentassets", StringComparison.OrdinalIgnoreCase))
                return true;

            if (path.StartsWithSegments("/globalassets", StringComparison.OrdinalIgnoreCase))
                return true;

            if (path.StartsWithSegments("/siteassets", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }

    public class ImageRequestSigningOptions
    {
        public bool? Enabled { get; set; }

        public string Salt { get; set; }
    }
}
