using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Api.Factories;
using Nop.Api.Models.Customer;
using Nop.Api.Models.Directory;
using Nop.Services.Customers;

namespace Nop.Api.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/country")]
    [ApiController]
    public class CountryController : BaseApiController
    {
        #region Fields
        private readonly ICustomerService _customerService;
        private readonly ICountryModelFactory _countryModelFactory;

        #endregion

        #region Ctor

        public CountryController(
            ICustomerService customerService,
            ICountryModelFactory countryModelFactory
        ) : base(customerService)
        {
            _customerService = customerService;
            _countryModelFactory = countryModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrive states by country id
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="addSelectStateItem"></param>
        /// <returns></returns>
        /// <remarks>
        /// Simple request:
        /// GET api/v1/country/getstatesbycountryid?countryId=491&addSelectStateItem=true
        /// </remarks>
        [HttpGet("getstatesbycountryid")]
        [ProducesResponseType(typeof(IEnumerable<StateProvinceModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            var model = await _countryModelFactory.GetStatesByCountryIdAsync(countryId, addSelectStateItem);
            if (model == null)
                return NotFound();
            return Ok(model);
        }

        #endregion
    }
}
