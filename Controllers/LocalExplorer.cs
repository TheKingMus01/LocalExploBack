using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System;
using System.Threading.Tasks;
using WebApplication1.Interfaces;
using WebApplication1.Repository;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalExplorerController : ControllerBase
    {
        private readonly IGeolocationService _geolocationService;
        private readonly IActivity _activity;
        private readonly IConfiguration _configuration;

        public LocalExplorerController(IGeolocationService geolocationService, IActivity activity, IConfiguration configuration)
        {
            _geolocationService = geolocationService;
            _activity = activity;
            _configuration = configuration;
        }

        [HttpGet("geolocation")]
        public async Task<IActionResult> GetGeolocation()
        {
            try
            {
                var weatherInfo = await _geolocationService.GetWeatherAsync();
                return Ok(weatherInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("ActivitySuggestions")]
        public async Task<IActionResult> GetChatResponse()
        {
            try
            {
                var response = await _activity.GetChatResponseAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
