using Website.Core.Models;

namespace Website.ViewComponents.ModularBlock
{
    public class ModularBlockViewModel
    {
        public bool IsPartial { get; set; }

        public string? name { get; set; }
        public Type? componentType { get; set; }

        public object model { get; set; }

        public PageContextModel? context {  get; set; }
    }
}
