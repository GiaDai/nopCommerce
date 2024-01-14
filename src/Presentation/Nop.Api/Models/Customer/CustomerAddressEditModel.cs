using Nop.Web.Framework.Models;
using Nop.Api.Models.Common;

namespace Nop.Api.Models.Customer
{
    public partial record CustomerAddressEditModel : BaseNopModel
    {
        public CustomerAddressEditModel()
        {
            Address = new AddressModel();
        }
        
        public AddressModel Address { get; set; }
    }
}