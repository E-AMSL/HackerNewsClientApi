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
    private readonly int maxAmount;

    public HackerNewsController(ILogger<HackerNewsController> logger, IHackerNewsService hackerNewsService, IConfiguration configuration)
    {
        this.logger = logger;
        this.hackerNewsService = hackerNewsService;

        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        maxAmount = appSettings?.MaxStoriesAmount ?? 200;  //max returned by https://hacker-news.firebaseio.com/v0/beststories.json is 200
    }

    [HttpGet("{amount}")]
    public async Task<ActionResult<IEnumerable<HackerNewsStory>>> Get(int amount)
    {
        if (amount == 0)
        {
            return Ok(Enumerable.Empty<HackerNewsStory>());
        }

        if (amount > maxAmount)
        {
            return BadRequest($"Can't request for more than {maxAmount} items");
        }

        logger.LogInformation("GET - HackerNews - Get best stories from HackerNews API");
        ServiceResponse<IAsyncEnumerable<HackerNewsStory>> getNewsResult = await hackerNewsService.GetHackerNewsStoriesAsync(amount);

        if (!getNewsResult.Success)
        {
            logger.LogError(getNewsResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        if (getNewsResult.Response is null || !(await getNewsResult.Response.AnyAsync()))
        {
            return NotFound();
        }

        return Ok(getNewsResult.Response);
    }
}
