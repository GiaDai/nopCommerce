using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Api.Factories;
using Nop.Api.Models.Order;
using Nop.Services.Customers;

namespace Nop.Api.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/order")]
    [ApiController]
    public class OrderController : BaseApiController
    {
        #region Fields
        private readonly IOrderModelFactory _orderModelFactory;

        #endregion

        #region Ctor

        public OrderController(
            ICustomerService customerService,
            IOrderModelFactory orderModelFactory
        ) : base(customerService)
        {
            _orderModelFactory = orderModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get customer orders
        /// </summary>
        /// <returns></returns>
        [HttpGet("my")]
        [ProducesResponseType(typeof(CustomerOrderListModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MyOrders()
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();
            var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync(customer);
            if (model == null)
                return NotFound();
            return Ok(model);
        }

        #endregion
    }
}
