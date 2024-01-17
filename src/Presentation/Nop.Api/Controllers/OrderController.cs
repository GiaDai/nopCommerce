using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Api.Factories;
using Nop.Api.Models.Order;
using Nop.Core;
using Nop.Core.Domain.Customers;
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
        private readonly ICustomerService _customerService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly RewardPointsSettings _rewardPointsSettings;

        #endregion

        #region Ctor

        public OrderController(
            ICustomerService customerService,
            IOrderModelFactory orderModelFactory,
            RewardPointsSettings rewardPointsSettings
        ) : base(customerService)
        {
            _customerService = customerService;
            _orderModelFactory = orderModelFactory;
            _rewardPointsSettings = rewardPointsSettings;
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

        /// <summary>
        /// Get reward points history
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet("rewardpoints/history")]
        [ProducesResponseType(typeof(CustomerRewardPointsModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CustomerRewardPoints(int? pageNumber)
        {
            var customer = await GetCustomer();
            if (customer == null)
                return Unauthorized();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            if (!_rewardPointsSettings.Enabled)
                return BadRequest();

            var model = await _orderModelFactory.PrepareCustomerRewardPointsAsync(customer,pageNumber);
            return Ok(model);
        }
        #endregion
    }
}
