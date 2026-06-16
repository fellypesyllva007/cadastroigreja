using Microsoft.AspNetCore.Mvc;

namespace CadastroIgreja.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login()
    {
        return Ok(new { token = "jwt-placeholder" });
    }
}