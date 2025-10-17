using Microsoft.AspNetCore.Mvc;
using WeatherForecastSrvc.DataTransferObject;
using WeatherForecastSrvc.Services;

namespace WeatherForecastSrvc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _service;

        /// <summary>
        /// Constructor to initialize the LocationService.
        /// </summary>
        /// <param name="service"></param>
        public LocationController(LocationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Adds a new location (latitude/longitude). 
        /// If it already exists, returns the existing one.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddLocation(
            [FromBody] AddLocationRequest request,
            CancellationToken cancelToken)
        {
            var location = await _service.AddLocationAsync(request.Latitude, request.Longitude, cancelToken);
            return Ok(location);
        }


        /// <summary>
        /// Retrives all locations.
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllLocations(CancellationToken cancelToken)
        {
            var locations = await _service.GetAllLocationsAsync(cancelToken);

            if (locations == null || !locations.Any())
                return NotFound(new { message = "No locations found." });

            return Ok(locations);
        }


        /// <summary>
        /// Deletes a location by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteLocation(int id, CancellationToken cancelToken)
        {
            bool deleted = await _service.DeleteLocationAsync(id, cancelToken);

            if (!deleted)
                return NotFound(new { message = $"Location with ID {id} not found." });

            return NoContent();
        }

    }
}
