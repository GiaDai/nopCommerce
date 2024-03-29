﻿using Nop.Api.Models.Order;
using Nop.Core.Domain.Customers;

namespace Nop.Api.Factories
{
    /// <summary>
    /// Represents the interface of the order model factory
    /// </summary>
    public interface IOrderModelFactory
    {
        /// <summary>
        /// Prepare the customer order list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer order list model
        /// </returns>
        Task<CustomerOrderListModel> PrepareCustomerOrderListModelAsync(Customer customer);
    }
}
