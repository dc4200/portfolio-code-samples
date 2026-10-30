using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using System.Reflection;
using Website.Core.Attributes;
using Website.Core.ContentStack;
using Website.Core.Models;

namespace Website.ViewComponents.ModularBlock
{
    /// <summary>
    /// View Component for handling the rendering of Modular Block components
    /// </summary>
    public class ModularBlockViewComponent : ViewComponent
    {
        private readonly static Dictionary<string, Type> blockRegistry = new Dictionary<string, Type>();
        private readonly static Dictionary<string, Type> specialBlockRegistry = new Dictionary<string, Type>();

        // Static constructor to populate block registry once on startup with reflection
        static ModularBlockViewComponent()
        {
            var components = typeof(ModularBlockViewComponent).Assembly.GetTypes()
                .Where(x => x.IsAssignableTo(typeof(ViewComponent)) && x.CustomAttributes.Any(a => a.AttributeType.IsAssignableTo(typeof(IModularBlockDescriptor))));

            foreach (var component in components)
            {
                RegisterComponent<ModularBlockAttribute>(component, blockRegistry);
                RegisterComponent<SpecialContentBlockAttribute>(component, specialBlockRegistry);
            }
        }

        private static void RegisterComponent<T>(Type component, Dictionary<string, Type> registry)
            where T: Attribute, IModularBlockDescriptor
        {
            var attr = component.GetCustomAttribute<T>() as IModularBlockDescriptor;
            if (attr != null && !string.IsNullOrEmpty(attr.PropertyName))
            {
                // Safety check
                if (registry.ContainsKey(attr.PropertyName))
#pragma warning disable
                    // The exception here is intentional as it is a coding mistake to have more than one ViewComponent tagged for the same modular block
                    //  property.  This is intended to BREAK IT during debug session so you can FIX THE ISSUE
                    throw new InvalidDataException($"Modular Block Key {attr.PropertyName} already registered to type {blockRegistry[attr.PropertyName]}.  {component.Name} is a duplicate");
#pragma warning enable

                // Include in registry
                registry.Add(attr.PropertyName, component);
            }
        }

        public async Task<IViewComponentResult> InvokeAsync(GeneralModularContentBlock content, PageContextModel? context = null)
        {
            // The way ContentStack populates Modular blocks, one or more of the block containers may have one or more block entry
            // in it.  You have to check each block entry and add a rendering case to handle it
            // We can only return ONE partial from the view component so we will pass it a dictionary of partials and models
            //  and let it dynamically pull in the partial blocks
            var partials = new List<ModularBlockViewModel>();

            if (content?.general_content_blocks != null)
            {
                foreach (var block in content.general_content_blocks)
                {                    
                    // Special Code Generation functional block that is not configurable
                    if (!string.IsNullOrEmpty(block.special_content?.special_content_type))
                    {
                        if (specialBlockRegistry.TryGetValue(block.special_content.special_content_type, out var specialBlock))
                        {
                            AddComponent(partials, specialBlock, block.special_content, context);
                        }
                    }
                    else
                    {
                        // Check for any populated block properties matching the registry
                        var keys = blockRegistry.Keys.Where(k => block.HasProperty(k));
                        foreach (var key in keys)
                        {
                            var value = block.TryGetPropertyValue<object>(key);
                            if (value != null)
                            {
                                AddComponent(partials, blockRegistry[key], value, context);
                            }
                        }
                    }
                       
                }
            }

            return View(partials);
        }
        
        private static void AddComponent(List<ModularBlockViewModel> partials, Type componentType, object? blockModel, PageContextModel? context)
        {
            if (blockModel != null && componentType != null)
                partials.Add(new ModularBlockViewModel() { IsPartial = false, componentType = componentType, model = blockModel, context = context });
        }

        private static void AddBlock(List<ModularBlockViewModel> partials, string partialName, object? blockModel)
        {
            if (blockModel != null && !string.IsNullOrEmpty(partialName))
                partials.Add(new ModularBlockViewModel() { IsPartial = true, name = partialName, model = blockModel });
        }
    }
}
