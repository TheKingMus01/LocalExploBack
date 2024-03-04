using Microsoft.Extensions.Configuration;
using System.Text.Json;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using System.Net.Http;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Repository
{
    public class Activity : IActivity
    {
        private readonly IOpenAIService _openAiService;
        private readonly IGeolocationService _geolocationService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public Activity(IOpenAIService openAiService, IGeolocationService geolocationService, HttpClient httpClient, IConfiguration configuration)
        {
            _openAiService = openAiService ?? throw new ArgumentNullException(nameof(openAiService));
            _geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<ActivitySuggest>> GetChatResponseAsync()
        {
            var activityString = "";
            var weatherInfo = await _geolocationService.GetWeatherAsync();
            var weatherObject = JsonSerializer.Deserialize<WeatherInfo>(weatherInfo);
            var status = weatherObject.is_day ? "day" : "night";
            var completionRequest = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("You are a helpful assistant."),
                    ChatMessage.FromUser($"i feel so much bored , i need something to do , My city : {weatherObject.city} , my weather is {weatherObject.text}, " +
                    $"today date && time : {weatherObject.localtime}, and the Temperature : {weatherObject.temp_c} C " +
                    $"and i want you to give me 5 {status} Activity Suggestions (one line text) each one with a title or topic givng by you , " +
                    "and i need the Activity and their title to be separeted with : from eachothers , " +
                    "and the Activity suggestions to be separeted with ; at the end of each, " +
                    "and in your response i dont want to see anything but the list of activitys with their titles or topics ," +
                    "and i dont want '\n' dont retun to new line at all keep all in 1 line," +
                    "i want your answers to be so unique never been told before, decorated with emojis, with details")
                },
                Model = "gpt-3.5-turbo"
            };

            try
            {
                var completionResult = await _openAiService.ChatCompletion.CreateCompletion(completionRequest);

                if (completionResult.Successful)
                {
                    activityString = completionResult.Choices.FirstOrDefault()?.Message.Content;
                    var activities = activityString.Split(';').Select(activity => activity.Trim()).ToList();
                    var placesResponse = await GetPlace(activities);
                    var activitySuggestions = new List<ActivitySuggest>();

                    for (int i = 0; i < activities.Count && i < placesResponse.Count; i++)
                    {
                        var plusInfo = await this.GetPlacesRes(placesResponse[i]);

                        activitySuggestions.Add(new ActivitySuggest
                        {
                            Activity = activities[i],
                            Place = new Place { 
                                name = placesResponse[i] ,
                                image = plusInfo[0],
                                Description = plusInfo[1],
                                url = plusInfo[2],
                            }
                        });
                    }

                    return activitySuggestions;
                }
                else
                {
                    throw new Exception($"{completionResult.Error?.Code}: {completionResult.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<string>> GetPlace(List<string> activities)
        {
            var weatherInfo = await _geolocationService.GetWeatherAsync();
            var weatherObject = JsonSerializer.Deserialize<WeatherInfo>(weatherInfo);
            var status = weatherObject.is_day ? "day" : "night";
            var completionRequest = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("You are a helpful assistant."),
                    ChatMessage.FromUser($"based on this seggestions and my informations : " +
                    $"My city : {weatherObject.city} , my weather is {weatherObject.text}, it {status}" +
                    $"today date && time : {weatherObject.localtime}, and the Temperature : {weatherObject.temp_c} C  " +
                    $"give me 1 specific famus place in My city to do this activity for each activity in the same order of the activity {string.Join(";", activities)}" +
                    $"i just need the specific names of places with no adding information so i can search each in google maps" +
                    $", those names are separated with a '@' at the end of each, and in your response i dont want to see anything but the list of names of the places" +
                    $"and i dont want '\n' dont retun to new line at all keep all in 1 line,"
                    )
                },
                Model = "gpt-3.5-turbo"
            };

            try
            {
                var completionResult = await _openAiService.ChatCompletion.CreateCompletion(completionRequest);

                if (completionResult.Successful)
                {
                    var placesResponse = completionResult.Choices.FirstOrDefault()?.Message.Content;
                    return placesResponse.Split('@').Select(place => place.Trim()).ToList();
                }
                else
                {
                    throw new Exception($"{completionResult.Error?.Code}: {completionResult.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<string>> GetPlacesRes(string name)
        {
            var apiKey = _configuration["Maps:apikey"] ?? "";

            var getPlaceId_Image = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?fields=place_id%2Cphotos&input={name}&inputtype=textquery&key={apiKey}");
            getPlaceId_Image.EnsureSuccessStatusCode();
            var content = await getPlaceId_Image.Content.ReadAsStringAsync();
            var photoInfo = JsonSerializer.Deserialize<PhotoInfo>(content);
            var imageurl = "";
            var url = "";
            var Description = "Not Found";
            if (photoInfo.candidates != null) {
                var candidate = photoInfo.candidates[0];
                if (candidate.photos != null && candidate.photos.Count() > 0)
                {
                    var photos = candidate.photos[0];
                    var photoReference = photos.photo_reference;
                    var getImage = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&photoreference={photoReference}&key={apiKey}");
                    getImage.EnsureSuccessStatusCode();
                    imageurl = getImage.RequestMessage.RequestUri.ToString();
                }
                var place_id = candidate.place_id;
                var getURL = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/place/details/json?place_id={place_id}&key={apiKey}");
                getURL.EnsureSuccessStatusCode();
                var contentUrl = await getURL.Content.ReadAsStringAsync();
                var placeDetails = JsonSerializer.Deserialize<PlaceDetails>(contentUrl);
                url = placeDetails.result.url;
                Description = placeDetails.result.vicinity;
            }

            var PlusPlace = new List<string>
            {
                imageurl,
                Description,
                url
            };

            return PlusPlace;
        }

    }
}
