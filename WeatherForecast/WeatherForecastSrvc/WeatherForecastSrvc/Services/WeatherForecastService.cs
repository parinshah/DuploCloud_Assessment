using System.Text.Json;
using WeatherForecastSrvc.DataTransferObject;
using WeatherForecastSrvc.Model;

namespace WeatherForecastSrvc.Services
{
    public class WeatherForecastService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;


        /// <summary>
        /// Contructor initialize the httpClient object and gets the baseURL from config or else sets default.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="configuration"></param>
        public WeatherForecastService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["WeatherApi:BaseUrl"]
                       ?? "https://api.open-meteo.com/v1/"; // fallback if config missing
        }


        /// <summary>
        /// Fetches the current weather for given latitude and longitude from Open-Meteo API.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<Model.WeatherForecast?> GetCurrentWeatherAsync(
            double latitude, double longitude,
            CancellationToken cancelToken)
        {
            //Build the full request URL dynamically
            var url = $"{_baseUrl}forecast?latitude={latitude}&longitude={longitude}&current_weather=true";

            try
            {
                //Send the request
                var response = await _httpClient.GetAsync(url, cancelToken);
                
                var raw = await response.Content.ReadAsStringAsync(cancelToken);
                Console.WriteLine(raw);  // log full JSON response
                var fc = JsonSerializer.Deserialize<WeatherForecast>(raw);
                //Check success status
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Weather API returned {response.StatusCode}");
                    return null;
                }

                //Deserialize into ForecastResponse
                var forecast = await response.Content.ReadFromJsonAsync<Model.WeatherForecast>(cancellationToken: cancelToken);
                return forecast;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Weather API request was cancelled.");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network or connection error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching weather: {ex.Message}");
                return null;
            }
        }
    }
}
