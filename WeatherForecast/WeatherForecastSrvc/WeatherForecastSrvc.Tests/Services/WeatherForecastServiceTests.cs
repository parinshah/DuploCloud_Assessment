using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using WeatherForecastSrvc.Services;

namespace WeatherForecastSrvc.Tests.Services
{
    public class WeatherForecastServiceTests
    {
        #region Helper Methods

        private HttpClient CreateHttpClient(HttpResponseMessage responseMessage)
        {
            // Create an HttpClient with a fake handler that always returns a given response
            return new HttpClient(new FakeHttpMessageHandler(responseMessage))
            {
                BaseAddress = new Uri("https://api.open-meteo.com/v1/")
            };
        }

        private IConfiguration GetConfiguration(string? baseUrl = null)
        {
            // Build an in-memory configuration dictionary to simulate appsettings
            var dict = new Dictionary<string, string>();
            if (baseUrl is not null)
                dict["WeatherApi:BaseUrl"] = baseUrl;

            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
        }

        #endregion

        // ----------------------------------------------------------------------

        [Fact(DisplayName = "Should return forecast when API responds successfully")]
        public async Task GetCurrentWeatherAsync_ReturnsForecast_OnSuccess()
        {
            // Arrange
            var json = "{\"latitude\":1,\"longitude\":2,\"timezone\":\"UTC\",\"current_weather\":null}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var client = CreateHttpClient(response);
            var svc = new WeatherForecastService(client, GetConfiguration());

            // Act
            var result = await svc.GetCurrentWeatherAsync(1, 2, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Latitude);
            Assert.Equal(2, result.Longitude);
            Assert.Equal("UTC", result.Timezone);
        }

        // ----------------------------------------------------------------------

        [Fact(DisplayName = "Should return null when API returns non-success status code")]
        public async Task GetCurrentWeatherAsync_ReturnsNull_OnNonSuccess()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(string.Empty)
            };

            var client = CreateHttpClient(response);
            var svc = new WeatherForecastService(client, GetConfiguration());

            // Act
            var result = await svc.GetCurrentWeatherAsync(1, 2, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        // ----------------------------------------------------------------------

        [Fact(DisplayName = "Should return null when network error (HttpRequestException) occurs")]
        public async Task GetCurrentWeatherAsync_ReturnsNull_OnNetworkError()
        {
            // Arrange
            var client = new HttpClient(new ThrowingHandler(new HttpRequestException("Network unreachable")));
            var svc = new WeatherForecastService(client, GetConfiguration());

            // Act
            var result = await svc.GetCurrentWeatherAsync(12.3, 45.6, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        // ----------------------------------------------------------------------

        [Fact(DisplayName = "Should return null when API response contains malformed JSON")]
        public async Task GetCurrentWeatherAsync_ReturnsNull_OnMalformedJson()
        {
            // Arrange
            var badJson = "{ this is not valid json }";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(badJson, Encoding.UTF8, "application/json")
            };

            var client = CreateHttpClient(response);
            var svc = new WeatherForecastService(client, GetConfiguration());

            // Act
            var result = await svc.GetCurrentWeatherAsync(1, 2, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        // ----------------------------------------------------------------------

        [Fact(DisplayName = "Should return null when cancellation token is triggered")]
        public async Task GetCurrentWeatherAsync_ReturnsNull_OnCancellation()
        {
            // Arrange
            var client = new HttpClient(new ThrowingHandler(new TaskCanceledException()));
            var svc = new WeatherForecastService(client, GetConfiguration());
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await svc.GetCurrentWeatherAsync(1, 2, cts.Token);

            // Assert
            Assert.Null(result);
        }

        // ----------------------------------------------------------------------

        [Fact(DisplayName = "Should use default base URL when configuration missing")]
        public async Task GetCurrentWeatherAsync_UsesDefaultBaseUrl_WhenConfigMissing()
        {
            // Arrange
            var handler = new InspectingHandler();
            var client = new HttpClient(handler);
            var svc = new WeatherForecastService(client, GetConfiguration());

            // Act
            await svc.GetCurrentWeatherAsync(1, 2, CancellationToken.None);

            // Assert
            Assert.NotNull(handler.RequestedUri);
            Assert.Contains("https://api.open-meteo.com/v1/", handler.RequestedUri!.ToString());
        }

        // ----------------------------------------------------------------------
        #region Test Helpers

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                // Clone minimal parts to avoid shared state across tests
                _response = new HttpResponseMessage(response.StatusCode)
                {
                    Content = response.Content,
                    ReasonPhrase = response.ReasonPhrase,
                    Version = response.Version
                };
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_response);
        }

        private class ThrowingHandler : HttpMessageHandler
        {
            private readonly Exception _toThrow;
            public ThrowingHandler(Exception toThrow) => _toThrow = toThrow;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw _toThrow;
        }

        private class InspectingHandler : HttpMessageHandler
        {
            public Uri? RequestedUri { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestedUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"latitude\":1,\"longitude\":2}")
                });
            }
        }

        #endregion
    }
}
