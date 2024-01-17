using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Api.Factories;
using Nop.Api.Models.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Api.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/product")]
    [ApiController]
    public class ProductController : BaseApiController
    {
        #region Fields
        private readonly IAclService _aclService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IStoreMappingService _storeMappingService;
        #endregion

        #region Ctor

        public ProductController(
            IAclService aclService,
            ICustomerService customerService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IStoreMappingService storeMappingService
            ) : base(customerService)
        {
            _aclService = aclService;
            _customerService = customerService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get product for home page
        /// </summary>
        /// <param name="productThumbPictureSize"></param>
        /// <returns></returns>
        [HttpGet("homepage")]
        [ProducesResponseType(typeof(List<ProductOverviewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HomepageProducts(int? productThumbPictureSize)
        {
            var products = await (await _productService.GetAllProductsDisplayedOnHomepageAsync())
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            if (!products.Any())
                return NotFound();

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

            return Ok(model);
        }

        #endregion
    }
}
