using Microsoft.AspNetCore.Mvc;
namespace CadastroIgreja.Api.Controllers;
[ApiController]
[Route("api/igrejas")]
public class IgrejasController:ControllerBase
{
 [HttpGet]
 public IActionResult Get()=>Ok();
}