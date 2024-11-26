using HackerNewsClient.Api.Models;
using HackerNewsClient.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsClient.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HackerNewsController : Controller
{
    private readonly ILogger<HackerNewsController> logger;
    private readonly IHackerNewsService hackerNewsService;
    private readonly int maxAmount = 200; //max returned by https://hacker-news.firebaseio.com/v0/beststories.json

    public HackerNewsController(ILogger<HackerNewsController> logger, IHackerNewsService hackerNewsService)
    {
        this.logger = logger;
        this.hackerNewsService = hackerNewsService;
    }

    [HttpGet("{amount}")]
    public async Task<ActionResult<IEnumerable<HackerNewsStory>>> Get(int amount)
    {
        if (amount > maxAmount)
        {
            return BadRequest("Can't request for more than 200 items");
        }

        logger.LogInformation("GET - HackerNews - Get best stories from HackerNews API");
        ServiceResponse<IAsyncEnumerable<HackerNewsStory>> getNewsResult = await hackerNewsService.GetHackerNewsStoriesAsync(amount);

        if (!getNewsResult.Success)
        {
            logger.LogError(getNewsResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        if (getNewsResult.Response is null)
        {
            return NotFound();
        }

        return Ok(getNewsResult.Response);
    }
}
