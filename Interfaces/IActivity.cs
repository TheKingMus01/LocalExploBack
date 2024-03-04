using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface IActivity
    {
        Task<List<ActivitySuggest>> GetChatResponseAsync();

    }
}
