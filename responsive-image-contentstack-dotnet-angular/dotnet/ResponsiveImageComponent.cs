using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Website.Abstractions;
using Website.Core.Attributes;
using Website.Core.ContentStack;
using Website.Components.ResponsiveImage;

namespace Website.Components.ResponsiveImage
{
    [ViewComponent]
    [ModularBlock("responsive_image")]
    public class ResponsiveImageViewComponent(ILogger<ResponsiveImageViewComponent> logger) : ViewComponent
    {

        public async Task<IViewComponentResult> InvokeAsync(
            Website.Core.ContentStack.ResponsiveImage content,

            // dual use: programmatic + Contentstack
            //CSFile? image = null,
            //bool? isEagerLoad = null,
            //bool? isFullWidth = null,
            //string? caption = null,
            //ImageLink? link = null,
            //List<ImageOverrideContainerViewModel>? overrides = null,
            //string? aspectRatio = null,
            //bool? isSmart = null,

            CancellationToken c = default)
        {
            if (content != null)
            {
                try
                {
                    var viewModel = new ResponsiveImageViewModel
                    {
                        // dual use: programmatic + Contentstack
                        //image = image ?? content.default_image,
                        //isEagerLoad = isEagerLoad ?? content.is_eager_load,
                        //isFullWidth = isFullWidth ?? content.is_full_width,
                        //caption = caption ?? content.caption,
                        //link = link ?? content.link,
                        //aspectRatio = aspectRatio ?? content.aspect_ratio,
                        //overrides = overrides ?? new List<ImageOverrideContainerViewModel>(),
                        //isSmart = isSmart ?? content.is_smart,

                        // single use: Contentstack only
                        image = content.default_image,
                        isEagerLoad = content.is_eager_load,
                        isFullWidth = content.is_full_width,
                        caption = content.caption,
                        link = content.link,
                        aspectRatio = content.aspect_ratio,
                        overrides = new List<ImageOverrideContainerViewModel>(),
                        isSmart = content.is_smart,
                    };

                    if (content.overrides != null && content.overrides.Length > 0)
                    {
                        foreach (var imageOverrideContainer in content.overrides)
                        {
                            if (imageOverrideContainer.override_image_block != null)
                            {
                                var imageOverridesViewModel = new ImageOverrideViewModel
                                {
                                    image = imageOverrideContainer.override_image_block.image,
                                    targetScreen = imageOverrideContainer.override_image_block.target_screen,
                                    aspectRatio = imageOverrideContainer.override_image_block.aspect_ratio,
                                };

                                var containerViewModel = new ImageOverrideContainerViewModel
                                {
                                    overrideImageBlock = imageOverridesViewModel
                                };
                                viewModel.overrides.Add(containerViewModel);
                            }
                        }
                    }
                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failure Rendering Two Column Content Block");
                }
            }

            //Failsafe return of empty content
            return Content(string.Empty);
        }
    }
}
