using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;

namespace Nop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public BaseApiController(
            ICustomerService customerService
            )
        {
            _customerService = customerService;
        }

        protected async Task<Customer> GetCustomer()
        {
            string userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return null;
            var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(userId));
            return customer;
        }
    }
}
