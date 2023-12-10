namespace HackerNews
{
    using HackerNews.ConfigSettings;
    using HackerNews.Models;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using System.Net.Http;

    public class HackerService : IHackerService
    {
        private readonly IOptions<StoryUrlsAppSettings> _storyUrlsAppSettings;
        private readonly IOptions<CacheAppSettings> _cacheAppSettings;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
              
        private const string BestStoriesCacheKey = "BestStoriesCacheKey";

        // To handle simulataneous requests when Cache is empty, only trigger Cache population for single request.
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public HackerService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IOptions<StoryUrlsAppSettings> storyUrlsAppSettings,
            IOptions<CacheAppSettings> cacheAppSettings, IMemoryCache memoryCache)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(HackerService));
            _configuration = configuration;
            _storyUrlsAppSettings = storyUrlsAppSettings;
            _cacheAppSettings = cacheAppSettings;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Get Best n stories
        /// </summary>
        /// <param name="count">Required number of stories</param>
        /// <returns>Best n Stories</returns>
        /// <exception cref="Exception"></exception>
        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count)
        {
            // Check cache
            if (!_memoryCache.TryGetValue(BestStoriesCacheKey, out List<Story> stories))
            {
                try
                {
                    await _semaphore.WaitAsync();

                    // Another Check. In case the Cache get populated by another thread
                    if (!_memoryCache.TryGetValue(BestStoriesCacheKey, out stories))
                    {
                        stories = new List<Story>();

                        // Get IDs
                        var allStoriesUrl = _storyUrlsAppSettings.Value.BestStoriesUrl;
                        List<int> ids = new List<int>();

                        try
                        {
                            var response = await _httpClient.GetAsync(allStoriesUrl);
                            ids = await response.Content.ReadFromJsonAsync<List<int>>();
                        }
                        catch (Exception ex) 
                        {
                            throw new Exception("Error while fetching story ids.", ex);
                        }

                        // Fetch details for each story
                        var tasks = ids.Select(async s =>
                        {
                            Story story = await GetStoryAsync(s);
                            if (story != null)
                                stories.Add(story);
                        });

                        await Task.WhenAll(tasks);

                        // Sort according to the Score
                        stories.Sort();

                        // Populate Cache
                        var cacheEntryOptions = GetCacheOptions();

                        _memoryCache.Set(BestStoriesCacheKey, stories, cacheEntryOptions);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }                
            }

            return stories.Take(count);
        }

        private async Task<Story> GetStoryAsync(int id)
        {
            Story story = new Story();
            try
            {
                string url = _storyUrlsAppSettings.Value.StoryByIdUrl.Replace("{id}", id.ToString());

                var response = await _httpClient.GetAsync(url);

                story = await response.Content.ReadFromJsonAsync<Story>();
            }
            catch(Exception ex) 
            {
                throw new Exception($"Error while fetching details for Story ID: {id}.", ex);
            }

            return story;
        }

        private MemoryCacheEntryOptions GetCacheOptions()
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(_cacheAppSettings.Value.SlidingExpiration)) // Refresh if inactive for 3 hours
            .SetAbsoluteExpiration(TimeSpan.FromHours(_cacheAppSettings.Value.AbsoluteExpiration)) // Mandatory refresh every 24 hours
            .SetPriority(CacheItemPriority.Normal)
            .SetSize(1);

            return cacheEntryOptions;
        }
    }
}
