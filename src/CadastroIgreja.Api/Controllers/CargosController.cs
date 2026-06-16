using Microsoft.AspNetCore.Mvc;
namespace CadastroIgreja.Api.Controllers;
[ApiController]
[Route("api/cargos")]
public class CargosController:ControllerBase
{
 [HttpGet]
 public IActionResult Get()=>Ok();
}