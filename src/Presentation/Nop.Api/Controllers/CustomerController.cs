using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Api.Models.Customer;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;

namespace Nop.Api.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        public CustomerController(
            IConfiguration config
            , ICustomerRegistrationService customerRegistrationService
            , ICustomerService customerService)
        {
            _config = config;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;

        }
        // generate login token
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(model.Username?.Trim(), model.Password);
            var customer = await _customerService.GetCustomerByEmailAsync(model.Username?.Trim());
            await _customerRegistrationService.SignInCustomerAsync(customer, "/", true);
            var tokenString = GenerateJSONWebToken(customer);
            return Ok(new { token = tokenString });
        }

        private string GenerateJSONWebToken(Customer userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim("uid", userInfo.Id.ToString())
            };
            var token = new JwtSecurityToken(
              _config["JWTSettings:Issuer"],
              _config["JWTSettings:Issuer"],
              claims: claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
