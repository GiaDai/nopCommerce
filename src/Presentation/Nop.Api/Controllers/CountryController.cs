using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Api.Factories;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Api.Controllers
{
    [Authorize]
    [Route("api/country")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        #region Fields

        private readonly ICountryModelFactory _countryModelFactory;

        #endregion

        #region Ctor

        public CountryController(ICountryModelFactory countryModelFactory)
        {
            _countryModelFactory = countryModelFactory;
        }

        #endregion

        #region Methods

        #region States / provinces
        
        [HttpGet("getstatesbycountryid")]
        public virtual async Task<IActionResult> GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            var model = await _countryModelFactory.GetStatesByCountryIdAsync(countryId, addSelectStateItem);

            return Ok(model);
        }

        #endregion

        #endregion
    }
}
