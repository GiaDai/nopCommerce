using Nop.Api.Models.Catalog;
using Nop.Core.Domain.Customers;

namespace Nop.Api.Factories
{
    public interface IProductModelFactory
    {
        /// <summary>
        /// Prepare the customer product reviews model
        /// </summary>
        /// <param name="page">Number of items page; pass null to load the first page</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer product reviews model
        /// </returns>
        Task<CustomerProductReviewsModel> PrepareCustomerProductReviewsModelAsync(Customer customer,int? page);
    }
}
