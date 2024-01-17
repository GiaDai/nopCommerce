using Nop.Web.Framework.Models;

namespace Nop.Api.Models.Media
{
    public partial record VideoModel : BaseNopModel
    {
        public string VideoUrl { get; set; }

        public string Allow { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
