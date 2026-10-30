using System.Diagnostics;
using Website.Core.ContentStack;

namespace Website.Components.ResponsiveImage
{
    internal class ResponsiveImageViewModel
    {
        public CSFile? image { get; set; }
        public bool isEagerLoad { get; set; }
        public bool isFullWidth { get; set; }
        public string? caption { get; set; }
        public ImageLink? link { get; set; }
        public List<ImageOverrideContainerViewModel>? overrides { get; set; }
        public string? aspectRatio { get; set; }
        public bool isSmart { get; set; }

        // key responsive img vals
        private const int maxWidth = 1760;
        private const int HighDpiQualilty = 40;

        // sizes attr
        public string GetSizes() => isFullWidth
            ? $"(max-width: {maxWidth}px) 100vw, {maxWidth}px"
            : $"(max-width: {maxWidth}px) 50vw, {maxWidth / 2}px";

        // srcset attr for base (ie img el)
        public string GetSrcSetForBase()
        {
            if (string.IsNullOrEmpty(image?.url)) return string.Empty;
            return GenerateSrcSet(image.url, aspectRatio);
        }

        // srcset attr for override (ie source el)
        public string? GetSrcSetForOverride(string screenName)
        {
            var overrideBlock = overrides?.FirstOrDefault(overrideEl =>
                overrideEl.overrideImageBlock?.targetScreen == screenName)?.overrideImageBlock;

            if (overrideBlock?.image?.url == null) return null;

            return GenerateSrcSet(overrideBlock.image.url, overrideBlock.aspectRatio);
        }

        // src attr fallback
        public string GetDefaultSrc()
        {
            if (string.IsNullOrEmpty(image?.url)) return string.Empty;

            if (!string.IsNullOrEmpty(aspectRatio) && aspectRatio != "Intrinsic")
            {
                return $"{image.url}?width={maxWidth / 2}";
            }
            return $"{image.url}?width={maxWidth / 2}";
        }

        // shared logic for srcset creation
        private string GenerateSrcSet(string url, string? ratio)
        {
            int[] widths = { 320, 640, 768, 1024, 1280, 1536, maxWidth, (int)(maxWidth * 1.5), maxWidth * 2 };

            // if aspectRatio not intrinsic
            bool isFixedRatio = !string.IsNullOrEmpty(ratio) && ratio != "Intrinsic";

            string smartSuffix = isSmart ? ",smart" : string.Empty;

            var sources = widths.Select(width =>
            {
                // Contentstack Image Delivery API url params
                string highDpiParam = width > maxWidth ? $"&quality={HighDpiQualilty}" : string.Empty;
                string cropParam = isFixedRatio ? $"&crop={ratio}{smartSuffix}" : string.Empty;
                string formatParam = "&format=auto";

                return $"{url}?width={width}{cropParam}{highDpiParam}{formatParam} {width}w";
            });

            return string.Join(", ", sources);
        }

        // CSS-friendly aspect ratio
        public string GetCssAspectRatio(string? targetScreenName = null)
        {
            string? ratio = null;
            CSFile? targetImage = image;

            // look for override ratio
            if (!string.IsNullOrEmpty(targetScreenName) && (overrides is { Count: > 0 }))
            {
                var overrideBlock = overrides?.FirstOrDefault(o => o.overrideImageBlock?.targetScreen == targetScreenName)?.overrideImageBlock;

                if (overrideBlock != null) {
                    ratio = overrideBlock?.aspectRatio;
                    targetImage = overrideBlock?.image;
                }
            }

            // base ratio fallback
            if (string.IsNullOrEmpty(ratio))
            {
                ratio = aspectRatio;
            }

            // "Intrinsic", null or empty strs converted to "auto"
            // if img width & height available, calculate aspect ratio to prevent CLS
            if (string.IsNullOrWhiteSpace(ratio) ||
                ratio.Equals("Intrinsic", StringComparison.OrdinalIgnoreCase))
            {
                var width = targetImage?.dimension?.Width;
                var height = targetImage?.dimension?.Height;

                if (width > 0 && height > 0)
                {
                    return $"{width} / {height}";
                }

                return "auto";
            }

            // convert Contentstack aspect ratio to CSS aspect ratio (replace : with /)
            return ratio.Replace(":", "/");
        }

        // limit image size to max intrinsic width
        public string GetIntrinsicWidth(string? targetScreenName = null)
        {
           CSFile? targetImage = image;

           // look for override ratio
           if (!string.IsNullOrEmpty(targetScreenName) && (overrides is { Count: > 0 }))
           {
               var overrideBlock = overrides?.FirstOrDefault(o => o.overrideImageBlock?.targetScreen == targetScreenName)?.overrideImageBlock;
               if (overrideBlock?.image != null)
               {
                   targetImage = overrideBlock.image;
               }
           }

           // base ratio fallback
           var width = targetImage?.dimension?.Width;

           return width > 0 ? $"{width}px" : "auto";
        }

    }

    // dual use: programmatic + Contentstack
    //public class ImageOverrideContainerViewModel

    // single use: Contentstack only
    internal class ImageOverrideContainerViewModel
    {
        public ImageOverrideViewModel? overrideImageBlock { get; set; }
    }

    // dual use: programmatic + Contentstack
    //public class ImageOverrideViewModel

    // single use: Contentstack only
    internal class ImageOverrideViewModel
    {
        public CSFile? image { get; set; }
        public string? targetScreen { get; set; }
        public string? aspectRatio { get; set; }
    }
}
