using System;

using EPiServer;
using EPiServer.Core;
using EPiServer.Web.Routing;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Baaijte.Optimizely.ImageSharp.Web
{
    public static class HtmlHelperExtensions
    {
        public static UrlBuilder ProcessImage(this IHtmlHelper helper, ContentReference image)
        {
            if (image == null || image == ContentReference.EmptyReference)
                throw new ArgumentNullException(nameof(image), "You might want to use `ProcessImageWithFallback()` instead");

            var url = UrlResolver.Current.GetUrl(image);

            return ConstructUrl(url);
        }

        public static UrlBuilder ProcessImageWithFallback(this IHtmlHelper helper, ContentReference image, string imageFallback)
        {
            return ConstructUrl(image == null || image == ContentReference.EmptyReference ? imageFallback : UrlResolver.Current.GetUrl(image));
        }

        public static UrlBuilder ProcessImage(this IHtmlHelper helper, string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException(nameof(imageUrl), "You might want to use `ProcessImageWithFallback()` instead");

            return ConstructUrl(imageUrl);
        }

        public static UrlBuilder ProcessImageWithFallback(this IHtmlHelper helper, string imageUrl, string imageFallback)
        {
            return ConstructUrl(string.IsNullOrEmpty(imageUrl) ? imageFallback : imageUrl);
        }

        private static UrlBuilder ConstructUrl(string url)
        {
            var builder = new UrlBuilder(url);

            return builder;
        }
    }
}


