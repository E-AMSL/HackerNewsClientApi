using HackerNewsClient.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsClient.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HackerNewsController : Controller
{
    private readonly ILogger<HackerNewsController> logger;
    private readonly IHackerNewsService hackerNewsService;

    public HackerNewsController(ILogger<HackerNewsController> logger, IHackerNewsService hackerNewsService)
    {
        this.logger = logger;
        this.hackerNewsService = hackerNewsService;
    }

    [HttpGet(Name = "GetNews")]
    public async Task<ActionResult<IEnumerable<HnNews>>> Get()
    {
        var getNewsResult = await hackerNewsService.GetHnNews();

        if (!getNewsResult.Success)
        {
            logger.LogError(getNewsResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        if (getNewsResult.Result is null || !getNewsResult.Result.Any())
        {
            return NotFound();
        }

        return Ok(getNewsResult.Result);
    }
}
