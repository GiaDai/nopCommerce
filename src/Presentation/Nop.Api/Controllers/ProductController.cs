using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;

namespace Nop.Api.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/product")]
    [ApiController]
    public class ProductController : BaseApiController
    {
        #region Fields
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public ProductController(ICustomerService customerService) : base(customerService)
        {
            _customerService = customerService;
        }

        #endregion

        #region Product reviews

        

        #endregion
    }
}
