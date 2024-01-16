using Nop.Api.Models.Customer;
using Nop.Core.Domain.Customers;

namespace Nop.Api.Factories
{
    /// <summary>
    /// Represents the interface of the customer model factory
    /// </summary>
    public interface ICustomerModelFactory
    {
        /// <summary>
        /// Prepare the customer info model
        /// </summary>
        /// <param name="model">Customer info model</param>
        /// <param name="customer">Customer</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomCustomerAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer info model
        /// </returns>
        Task<CustomerInfoModel> PrepareCustomerInfoModelAsync(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "");

        /// <summary>
        /// Prepare the customer address list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer address list model  
        /// </returns>
        Task<CustomerAddressListModel> PrepareCustomerAddressListModelAsync();
        /// <summary>
        /// Prepare the change password model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the change password model
        /// </returns>
        Task<ChangePasswordModel> PrepareChangePasswordModelAsync();
    }
}
