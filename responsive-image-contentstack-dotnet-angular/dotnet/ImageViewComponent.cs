using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text.Encodings.Web;

namespace Website.Components.Image
{
    /// <summary>
    /// Attribute to declare named presets for image size generations.
    /// Syntax for each entry is: "name=320,640,1024" or "name:320,640,1024".
    /// The attribute strings are parsed at runtime by the component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ImageSizePresetsAttribute : Attribute
    {
        public string[] Presets { get; }

        public ImageSizePresetsAttribute(params string[] presets)
        {
            Presets = presets ?? Array.Empty<string>();
        }
    }

    public enum ImageFormat
    {
        png,
        jpeg,
        webp,
        avif
    }

    /// <summary>
    /// Renders an IMG element. It can build a srcset and sizes attributes from either:
    ///  - an explicit sizes array passed to the invocation, or
    ///  - a named preset defined via the ImageSizePresetsAttribute on this class.
    ///
    /// URL generation for each width appends a query parameter "w={width}".
    /// If the provided src contains a "?" the parameter is appended with "&".
    /// Additional arbitrary attributes can be passed via the 'attributes' parameter and are passed through
    /// to the generated img tag (excluding reserved attributes: src, alt, srcset, sizes, loading).
    /// </summary>
    [ImageSizePresets(
        "thumbnail=320,640",
        "responsive=480,768,1024,1365",
        "plan-tile=590,767,1279,1760"
    )]
    [ViewComponent(Name = "image")]
    public class ImageViewComponent : ViewComponent
    {
        // Public invocation:
        // src: required image URL or app-relative path (e.g. "~/images/photo.jpg")
        // format: output image format to request from CDN/pipeline
        // alt: alt text
        // preset: optional preset name declared via attribute (overridden by explicit sizes param)
        // sizes: optional explicit widths to generate srcset for (int[]). If provided, overrides preset.
        // cssClass: optional class attribute
        // loading: optional loading attribute value (default "lazy")
        // attributes: optional additional attributes passed through to the <img> tag
        public IViewComponentResult Invoke(
            string src,
            ImageFormat format = ImageFormat.webp,
            string alt = "",
            string sizes = null,
            string cssClass = null,
            bool isEager = false,
            int? intrinsicHeight = 0,
            int? intrinsicWidth = 0,
            int? height = null,
            string? ratio = null,
            int maxWidth = 1760)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                throw new ArgumentException("src must be provided", nameof(src));
            }

            int[] widths = { 320, 640, 768, 1024, 1280, 1536, maxWidth, (int)(maxWidth * 1.5), maxWidth * 2 };
            var loading = "lazy";
            string aspectRatio = "";
            bool isIntrinisicRatio = true;
            string fetchPriority = "auto";

            //determine if source of image is from ContentStack or ImageServer
            //if image comes from FD (not ContentStack) assume the aspect ratio is 16:9
            if (src.Contains("images.contentstack", StringComparison.OrdinalIgnoreCase))
            {
                if (ratio != null)
                {
                    aspectRatio = ratio;
                    isIntrinisicRatio = false;
                }
                else if (intrinsicHeight != null && intrinsicWidth != null)
                {
                    //fall back logic
                    aspectRatio = $"{intrinsicWidth}:{intrinsicHeight}";
                }

            } else
            {
                //front door images set defaulted ratio to 16:9
                aspectRatio = "16:9";
            }

            //if loading is lazy, set isEager flag to true
            if (isEager)
            {
                loading = "eager";
                fetchPriority = "high";
            }

            // Build attributes
            var srcAttr = Url.Content(src);

            // Use built in format support from CDN to specify image format
            srcAttr += $"?format={format.ToString()}";
                        
            var srcsetAttr = widths != null && widths.Length > 0 ? BuildSrcSet(srcAttr, widths, aspectRatio, isIntrinisicRatio, height) : null;
            var sizesAttr = sizes != null ? sizes : null;

            srcAttr += "&width=" + maxWidth;

            if (!isIntrinisicRatio)
            {
              srcAttr += $"&crop={aspectRatio}";
            }
            else if (height.HasValue && src.Contains("images.contentstack", StringComparison.OrdinalIgnoreCase))
            {
              srcAttr += $"&height={height.Value}&fit=crop";
            }

            // HTML-encode values that come from user input
            var encoder = HtmlEncoder.Default;
            var altEnc = encoder.Encode(alt ?? string.Empty);

            var classAttr = string.IsNullOrWhiteSpace(cssClass) ? string.Empty : $" class=\"{encoder.Encode(cssClass)}\"";
            var loadingAttr = string.IsNullOrWhiteSpace(loading) ? string.Empty : $" loading=\"{encoder.Encode(loading)}\"";
            var fetchAttr = string.IsNullOrWhiteSpace(fetchPriority) ? string.Empty : $" fetchpriority=\"{encoder.Encode(fetchPriority)}\"";
            var decodingAttr = $" decoding=\"async\"";
            var sizesAttrFormatted = string.IsNullOrEmpty(sizesAttr) ? string.Empty : $" sizes=\"{encoder.Encode(sizesAttr)}\"";
            var srcsetAttrFormatted = string.IsNullOrEmpty(srcsetAttr) ? string.Empty : $" srcset=\"{encoder.Encode(srcsetAttr)}\"";

            var html = $"<img src=\"{encoder.Encode(srcAttr)}\" alt=\"{altEnc}\"{classAttr}{loadingAttr}{fetchAttr}{decodingAttr}{srcsetAttrFormatted}{sizesAttrFormatted} />";

            return new HtmlContentViewComponentResult(new HtmlString(html));
        }

        // Build a srcset by appending ?w=WIDTH (or &w=WIDTH if query already present)
        private string BuildSrcSet(string src, int[] widths, string? aspectRatio, bool isIntrinsicRatio, int? height = null)
        {
            if (isIntrinsicRatio)
            {
                var pieces = widths.Select(w => $"{BuildUrlForWidth(src, w, null, isIntrinsicRatio, height)} {w}w");
                return string.Join(", ", pieces);
            }
            else
            {
                var pieces = widths.Select(w => $"{BuildUrlForWidth(src, w, aspectRatio, isIntrinsicRatio, height)} {w}w");
                return string.Join(", ", pieces);
            }
        }

        private string BuildUrlForWidth(string src, int width, string? ratio, bool isIntrinsic, int? height = null)
        {
            if (string.IsNullOrEmpty(src)) return src;

            string cropParam = "";

            //check where the image is coming from
            //if coming from Content Stack, check for Aspect Ratio
            if (src.Contains("images.contentstack", StringComparison.OrdinalIgnoreCase) && !isIntrinsic)
            {
                if (!string.IsNullOrEmpty(ratio))
                {
                    cropParam = $"&crop={ratio}";
                }
            }

            // If src already has a w= query param, replace it. Otherwise append.
            var separator = src.Contains('?') ? '&' : '?';
            // Very simple replace logic for an existing w= param
            var uriPart = src;
            var wKey = "w=";
            var idx = uriPart.IndexOf(wKey, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                // find end of value
                var end = uriPart.IndexOf('&', idx);
                if (end >= 0)
                {
                    uriPart = uriPart.Substring(0, idx) + $"{wKey}{width}" + uriPart.Substring(end);
                }
                else
                {
                    uriPart = uriPart.Substring(0, idx) + $"{wKey}{width}";
                }
                return uriPart;
            }
            if (isIntrinsic)
            {
              var url = $"{src}{separator}width={width}";
              if (height.HasValue && src.Contains("images.contentstack", StringComparison.OrdinalIgnoreCase))
              {
                url += $"&height={height.Value}&fit=crop";
              }
              return url;
            }
            else
            {
                return $"{src}{separator}width={width}{separator}crop={ratio}";
            }
        }
    }
}
