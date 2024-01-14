using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Api.Factories;
using Nop.Services.Customers;

namespace Nop.Api.Controllers
{
    [Authorize]
    [Route("api/order")]
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

        // My account/Orders
        [HttpGet("my")]
        public async Task<IActionResult> MyOrders()
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();
            var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync(customer);
            return Ok(model);
        }


        #endregion
    }
}
