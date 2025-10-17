using Microsoft.AspNetCore.Mvc;
using WeatherForecastSrvc.DataTransferObject;
using WeatherForecastSrvc.Services;

namespace WeatherForecastSrvc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly WeatherForecastService _weatherForecastService;
        private readonly LocationService _locationService;


        /// <summary>
        /// WeatherService — used to fetch data from Open-Meteo.
        /// LocationService — used to get stored coordinates from DB for /api/weather/{id}
        /// </summary>
        /// <param name="weatherForecastService"></param>
        /// <param name="locationService"></param>
        public WeatherForecastController(WeatherForecastService weatherForecastService, LocationService locationService)
        {
            _weatherForecastService = weatherForecastService;
            _locationService = locationService;
        }

        /// <summary>
        /// Get the current weather forecast for provided latitude and longitude.
        /// Example: GET /api/weatherforecast?latitude=78.88&longitude=-21.77
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWeatherForecastByCoordinates(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            CancellationToken cancelToken)
        {
            //Validate latitude and longitude ranges
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                return BadRequest(new { message = "Invalid latitude or longitude range." });

            //Call weather service
            var forecast = await _weatherForecastService.GetCurrentWeatherAsync(latitude, longitude, cancelToken);

            //Handle possible null or missing data
            if (forecast?.CurrentWeather == null)
                return NotFound(new { message = "Weather data not available." });

            //Map to Data transfer object for clean output
            var result = new WeatherForecastResult
            {
                Latitude = forecast.Latitude,
                Longitude = forecast.Longitude,
                Temperature = forecast.CurrentWeather.Temperature,
                Windspeed = forecast.CurrentWeather.Windspeed,
                Time = forecast.CurrentWeather.Time.ToString("u"),
                WindDirection = forecast.CurrentWeather.Winddirection,
                isDay = forecast.CurrentWeather.IsDay
            };

            //Return OK response
            return Ok(result);
        }

        /// <summary>
        /// Get the current weather forecast for a saved location by ID.
        /// Example: GET /api/weatherforecast/3
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetWeatherForecastByLocationId(int id, CancellationToken cancelToken)
        {
            //Look up location in DB
            var location = await _locationService.GetLocationByIdAsync(id, cancelToken);

            if (location == null)
                return NotFound(new { message = $"Location with ID {id} not found." });

            //Fetch forecast from weather service
            var forecast = await _weatherForecastService.GetCurrentWeatherAsync(location.Latitude, location.Longitude, cancelToken);

            if (forecast?.CurrentWeather == null)
                return NotFound(new { message = "Weather data not available." });

            //Map to Data transfer object for clean output
            var result = new WeatherForecastResult
            {
                Latitude = forecast.Latitude,
                Longitude = forecast.Longitude,
                Temperature = forecast.CurrentWeather.Temperature,
                Windspeed = forecast.CurrentWeather.Windspeed,
                Time = forecast.CurrentWeather.Time.ToString("u"),
                WindDirection = forecast.CurrentWeather.Winddirection,
                isDay= forecast.CurrentWeather.IsDay
            };

            return Ok(result);
        }
    }
}
