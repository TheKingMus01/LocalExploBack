namespace WebApplication1.Interfaces
{
    public interface IGeolocationService
    {
        Task<string> GetWeatherAsync();
    }
}
