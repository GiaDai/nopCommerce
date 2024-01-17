using Nop.Web.Framework.Models;

namespace Nop.Api.Models.Common
{
    public partial record CurrencyModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public string CurrencySymbol { get; set; }
    }
}