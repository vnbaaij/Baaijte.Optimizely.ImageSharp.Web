using System;

using EPiServer;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Processing;

namespace Baaijte.Optimizely.ImageSharp.Web
{
    public static class UrlBuilderExtensions
    {
        /// <summary>
        /// Changes the background color of the current image. This functionality is useful for adding a background when resizing image formats without an alpha channel.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">Either a hex rgb coded color (no #) or a named color</param>
        /// <returns></returns>
        public static UrlBuilder BackgroundColor(this UrlBuilder target, string color)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("bgcolor", color.ToLowerInvariant());

            return target;
        }

        /// <summary>
        /// Changes the background color of the current image. This functionality is useful for adding a background when resizing image formats with an alpha channel.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="color">Decimal red, green, lue and alpha values</param>
        /// <returns></returns>

        public static UrlBuilder BackgroundColor(this UrlBuilder target, int r, int g, int b, int a)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (r < 0 || g < 0 || b < 0 || a < 0 || r > 255 || g > 255 || b > 255 | a > 55)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("bgcolor", string.Join(",", r.ToString(), g.ToString(), b.ToString(), a.ToString()));

            return target;
        }



        /// <summary>
        /// Sets the output format of the current image to the given value.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="format"><see cref="ImageFormat"/></param>
        /// <returns></returns>
        public static UrlBuilder Format(this UrlBuilder target, ImageFormat format)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Set("format", format.ToString().ToLowerInvariant());

            return target;
        }

        /// <summary>
        /// Sets the output format to WebP if the browser accepts it.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static UrlBuilder FormatWebPIfAccepted(this UrlBuilder target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (!target.IsEmpty)
            {
                var context = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();
                context.HttpContext.Request.Headers.TryGetValue("Accept", out var acceptHeader);
                if (acceptHeader.ToString().Contains("image/webp"))
                {
                    target.QueryCollection.Set("format", ImageFormat.WebP.ToString().ToLowerInvariant());
                }
            }

            return target;
        }

        /// <summary>
        /// Alters the output quality of the current image. This method will only effect the output quality of images that allow lossy processing.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static UrlBuilder Quality(this UrlBuilder target, int quality)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (quality <= 0 || quality > 100)
                throw new ArgumentOutOfRangeException(nameof(target));

            if (!target.IsEmpty)
            {
                target.QueryCollection.Add("quality", quality.ToString());
            }

            return target;
        }

        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// ImageProcessor.Web allows you to scale images both up and down with an excellent quality to size ratio.A maximum width and height can be set in the configuration to help protect you from DoS attacks.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="width">New image width</param>
        /// <param name="height">New image height</param>
        /// <param name="mode">The resizing method.</param>
        /// <param name="center">The center position to anchor the image center point to.</param>
        /// <param name="anchor">The anchor position</param>
        /// <param name="compand">Whether to compress and expand individual pixel colors values to/from a linear color space when processing.</param>
        /// <returns></returns>
        public static UrlBuilder Resize(this UrlBuilder target, int? width, int? height, ResizeMode mode = ResizeMode.Pad, string sampler = null, string center = null, AnchorPositionMode anchor = AnchorPositionMode.Center, bool compand = false)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
            {
                if (width != null)
                    target.QueryCollection.Add("width", width.ToString());
                if (height != null)
                    target.QueryCollection.Add("height", height.ToString());
                if (mode != ResizeMode.Pad)
                    target.QueryCollection.Add("rmode", mode.ToString().ToLower());

                //if (sampler != null)
                target.QueryCollection.Add("rsampler", sampler ?? "bicubic");

                if (center != null)
                    target.QueryCollection.Add("rxy", center);
                if (anchor != AnchorPositionMode.Center)
                    target.QueryCollection.Add("ranchor", anchor.ToString().ToLower());
                if (!compand)
                    target.QueryCollection.Add("compand", "true");
            }
            return target;
        }

        /// <summary>
        /// Resizes the current image to the given width, Uses defaults for all other resize parameters
        /// </summary>
        /// <param name="target"></param>
        /// <param name="width">New width of the image</param>
        /// <returns></returns>
        public static UrlBuilder Width(this UrlBuilder target, int width)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("width", width.ToString());

            return target;
        }

        /// <summary>
        /// Resizes the current image to the given height, Uses defaults for all other resize parameters
        /// </summary>
        /// <param name="target"></param>
        /// <param name="height">New height of the image</param>
        /// <returns></returns>
        public static UrlBuilder Height(this UrlBuilder target, int height)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsEmpty)
                target.QueryCollection.Add("height", height.ToString());

            return target;
        }
    }
}
