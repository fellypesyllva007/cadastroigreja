using Microsoft.AspNetCore.Mvc;
namespace CadastroIgreja.Api.Controllers;
[ApiController]
[Route("api/usuarios")]
public class UsuariosController:ControllerBase
{
 [HttpGet]
 public IActionResult Get()=>Ok();
}