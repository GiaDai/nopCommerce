using Nop.Web.Framework.Models;

namespace Nop.Api.Models.Common
{
    public partial record LogoModel : BaseNopModel
    {
        public string StoreName { get; set; }

        public string LogoPath { get; set; }
    }
}