using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;

namespace Nop.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
        private readonly IConfiguration _config;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger
            , IConfiguration config
            , ICategoryService categoryService
            )
        {
            _logger = logger;
            _config = config;
            _categoryService = categoryService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var homepageCategories = await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("jwt", Name = "GetJwtConfig")]
        public IActionResult GetJwtConfig()
        {
            return Ok(new
            {
                Issuer = "Nop.Api",
                Audience = "Nop.Api",
                Key = _config["JWTSettings:Key"],
                AccessTokenExpiration = 60,
                RefreshTokenExpiration = 60 * 24 * 7
            });
        }
    }
}