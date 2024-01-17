using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Api.Factories;
using Nop.Api.Models.Catalog;
using Nop.Services.Customers;

namespace Nop.Api.Controllers
{
    /// <summary>
    /// Represents a catalog controller
    /// </summary>
    [Route("api/v{version:apiVersion}/catalog")]
    [ApiController]
    public class CatalogController : BaseApiController
    {
        #region Fields
        private readonly ICustomerService _customerService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        #endregion

        #region Ctor
        public CatalogController(
            ICustomerService customerService,
            ICatalogModelFactory catalogModelFactory
            
            ) : base(customerService)
        {
            _customerService = customerService;
            _catalogModelFactory = catalogModelFactory;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Retrieve root categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("root")]
        [ProducesResponseType(typeof(IEnumerable<CategorySimpleModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRootCategories()
        {
            var model = await _catalogModelFactory.PrepareRootCategoriesAsync();
            if (!model.Any())
                return NotFound();
            return Ok(model);
        }   

        /// <summary>
        /// Retrieve homepage categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("homepage")]
        [ProducesResponseType(typeof(IEnumerable<CategoryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHomepageCategories()
        {
            var model = await _catalogModelFactory.PrepareHomepageCategoryModelsAsync();
            if (!model.Any())
                return NotFound();
            return Ok(model);
        }

        #endregion
    }
}
