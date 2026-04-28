using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;

[ApiController]
[Route("/alliance")]
class Alliance : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateAlliance()
    {
        try
        {
            return Ok("Test");
        }
        catch(Exception e)
        {
            return BadRequest("Failed to create user: " + e.Message);
        }
    }
}
