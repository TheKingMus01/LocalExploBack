using Newtonsoft.Json;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Repository
{
    public class GeolocationService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeolocationService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
        }

        public async Task<string> GetWeatherAsync()
        {
            try
            {
                var city = await GetCityAsync();
                var weatherInfo = await WeatherInfoAsync(city);
                return weatherInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        private async Task<string> GetCityAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://ipinfo.io/json");
                response.EnsureSuccessStatusCode(); 
                var content = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(content);
                return jsonResponse.city;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to retrieve geolocation.", ex);
            }
        }

        private async Task<string> WeatherInfoAsync(string city)
        {
            try
            {
                var apiKey = _configuration["Weather:apikey"] ?? "";
                var response = await _httpClient.GetAsync($"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={city}&aqi=yes");
                response.EnsureSuccessStatusCode(); 

                var content = await response.Content.ReadAsStringAsync();
                var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(content);

                var weatherInfo = new WeatherInfo
                {
                    temp_c = weatherResponse.Current.TemperatureCelsius,
                    is_day = weatherResponse.Current.IsDay == 1,
                    text = weatherResponse.Current.Condition.Text,
                    icon = weatherResponse.Current.Condition.IconUrl,
                    localtime = weatherResponse.location.Localtime,
                    city = weatherResponse.location.name,
                };

                var json = JsonConvert.SerializeObject(weatherInfo);
                return json;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to retrieve weather data.", ex);
            }
        }
    }
}
