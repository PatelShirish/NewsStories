using HackerNews.Models;

namespace HackerNews
{
    public interface IHackerService
    {
        Task<IEnumerable<Story>> GetBestStoriesAsync(int count);
    }
}
