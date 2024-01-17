using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Api.Models.Common;

namespace Nop.Api.Models.Checkout
{
    public partial record CheckoutShippingAddressModel : BaseNopModel
    {
        public CheckoutShippingAddressModel()
        {
            ExistingAddresses = new List<AddressModel>();
            InvalidExistingAddresses = new List<AddressModel>();
            ShippingNewAddress = new AddressModel();
        }

        public IList<AddressModel> ExistingAddresses { get; set; }
        public IList<AddressModel> InvalidExistingAddresses { get; set; }
        public AddressModel ShippingNewAddress { get; set; }
        public bool NewAddressPreselected { get; set; }

        public int SelectedBillingAddress { get; set; }

        public bool DisplayPickupInStore { get; set; }
        public CheckoutPickupPointsModel PickupPointsModel { get; set; }
    }
}