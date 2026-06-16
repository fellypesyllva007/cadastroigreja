namespace CadastroIgreja.Application.Interfaces;
public interface IAuthService
{
 string GerarToken(string email);
}