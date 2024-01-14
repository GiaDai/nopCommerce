using Nop.Api.Models.Common;
using Nop.Web.Framework.Models;

namespace Nop.Api.Models.Customer
{
    public partial record CustomerAddressListModel : BaseNopModel
    {
        public CustomerAddressListModel()
        {
            Addresses = new List<AddressModel>();
        }

        public IList<AddressModel> Addresses { get; set; }
    }
}
