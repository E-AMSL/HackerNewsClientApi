using HackerNewsClient.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsClient.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HackerNewsController : Controller
{
    private readonly ILogger<WeatherForecastController> logger;
    private readonly IHackerNewsService hackerNewsService;

    public HackerNewsController(ILogger<WeatherForecastController> logger, IHackerNewsService hackerNewsService)
    {
        this.logger = logger;
        this.hackerNewsService = hackerNewsService;
    }

    [HttpGet(Name = "GetNews")]
    public ActionResult<IEnumerable<HnNews>> Get()
    {
        var getNewsResult = hackerNewsService.GetHnNews();

        if (!getNewsResult.Success)
        {
            logger.LogError(getNewsResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok(getNewsResult.Result);
    }
}
