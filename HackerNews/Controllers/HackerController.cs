using AutoMapper;
using HackerNews.Models;
using Microsoft.AspNetCore.Mvc;
namespace HackerNews.Controllers
{
    [ApiController]
    [Route("HackerNews")]
    public class HackerController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IHackerService _hackerService;

        public HackerController(IHackerService hackerService, IMapper mapper)
        {
            _hackerService = hackerService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets Best n Hacker News stories 
        /// </summary>
        /// <param name="count">Number of best stories</param>
        /// <returns>Best n stories</returns>
        [HttpGet]
        [Route("GetBestStories")]
        public async Task<IEnumerable<StoryResponse>> GetBestStories(int count = 10)
        {
            IEnumerable<Story> data = await _hackerService.GetBestStoriesAsync(count);
            return _mapper.Map<IEnumerable<StoryResponse>>(data);
        }
    }
}