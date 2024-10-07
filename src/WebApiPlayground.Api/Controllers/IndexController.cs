using Microsoft.AspNetCore.Mvc;

namespace WebApiPlayground.Api.Controllers;

[ApiController]
[Route("")]
public class IndexController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("It works!");
    }
}
