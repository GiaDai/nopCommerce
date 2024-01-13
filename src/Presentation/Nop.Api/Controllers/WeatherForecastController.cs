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
        private readonly ICategoryService _categoryService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger
            , ICategoryService categoryService
            )
        {
            _logger = logger;
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

        [HttpGet("GetCategory", Name = "GetCategory")]
        public async Task<IActionResult> GetCategory()
        {
            var homepageCategories = await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync(true);
            return Ok(homepageCategories);
        }
    }
}