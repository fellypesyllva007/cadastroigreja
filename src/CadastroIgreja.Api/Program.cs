using System.Security.Claims;
using System.Text.Encodings.Web;
using CadastroIgreja.Application;
using CadastroIgreja.Domain;
using CadastroIgreja.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication().AddInfrastructure();
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions, DemoBearerHandler>("Bearer", null);
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var auth = app.MapGroup("/api/auth").WithTags("Auth");
auth.MapPost("/register", async Task<Results<Created, BadRequest<string>>> (RegisterUserRequest request, AuthService service, CancellationToken ct) =>
{
    try
    {
        var id = await service.RegisterAsync(request, ct);
        return TypedResults.Created($"/api/users/{id}");
    }
    catch (InvalidOperationException ex)
    {
        return TypedResults.BadRequest(ex.Message);
    }
});

auth.MapPost("/login", async Task<Results<Ok<AuthTokenResponse>, UnauthorizedHttpResult>> (LoginRequest request, AuthService service, CancellationToken ct) =>
{
    var response = await service.LoginAsync(request, ct);
    return response is null ? TypedResults.Unauthorized() : TypedResults.Ok(response);
});

var churches = app.MapGroup("/api/churches").WithTags("Churches").RequireAuthorization();
churches.MapGet("/", async (Guid? parentId, ChurchType? type, ChurchService service, CancellationToken ct) =>
    TypedResults.Ok(await service.ListAsync(parentId, type, ct)));
churches.MapPost("/", async Task<Results<Created, BadRequest<string>>> (CreateChurchRequest request, ChurchService service, CancellationToken ct) =>
{
    try
    {
        var id = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/churches/{id}");
    }
    catch (InvalidOperationException ex)
    {
        return TypedResults.BadRequest(ex.Message);
    }
});

var users = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();
users.MapGet("/me", async Task<Results<Ok<UserProfileResponse>, NotFound>> (ClaimsPrincipal principal, UserService service, CancellationToken ct) =>
{
    var id = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var profile = await service.GetProfileAsync(id, ct);
    return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
});
users.MapPost("/{id:guid}/approve", async Task<Results<NoContent, NotFound>> (Guid id, UserService service, CancellationToken ct) =>
    await service.ApproveAsync(id, ct) ? TypedResults.NoContent() : TypedResults.NotFound());

app.MapPost("/api/role-requests", () => TypedResults.Created()).RequireAuthorization().WithTags("RoleRequests");
app.MapPost("/api/role-requests/{id:guid}/approve", (Guid id) => TypedResults.NoContent()).RequireAuthorization().WithTags("RoleRequests");
app.MapGet("/api/preacher-requests", () => TypedResults.Ok(Array.Empty<object>())).RequireAuthorization().WithTags("PreacherRequests");
app.MapPost("/api/preacher-requests", () => TypedResults.Created()).RequireAuthorization().WithTags("PreacherRequests");
app.MapPost("/api/preacher-requests/{id:guid}/approve", (Guid id) => TypedResults.Ok()).RequireAuthorization().WithTags("PreacherRequests");
app.MapGet("/api/letters", () => TypedResults.Ok(Array.Empty<object>())).RequireAuthorization().WithTags("Letters");
app.MapGet("/api/letters/{id:guid}/validate", (Guid id) => TypedResults.Ok(new { id, status = "Unknown" })).WithTags("Letters").AllowAnonymous();

app.Run();

internal sealed class DemoBearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (header.Equals("Bearer dev-admin", StringComparison.OrdinalIgnoreCase))
        {
            var devClaims = new[] { new Claim(ClaimTypes.NameIdentifier, Guid.Empty.ToString()), new Claim(ClaimTypes.Email, "admin@cadastroigreja.local") };
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(devClaims, Scheme.Name)), Scheme.Name)));
        }

        if (!header.StartsWith("Bearer demo.", StringComparison.OrdinalIgnoreCase)) return Task.FromResult(AuthenticateResult.NoResult());

        var tokenParts = header[7..].Split('.');
        if (tokenParts.Length != 3) return Task.FromResult(AuthenticateResult.Fail("Token inválido."));

        string[] payload;
        try
        {
            payload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1])).Split('|');
        }
        catch (FormatException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Token inválido."));
        }
        if (payload.Length < 2) return Task.FromResult(AuthenticateResult.Fail("Token inválido."));

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, payload[0]), new Claim(ClaimTypes.Email, payload[1]) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
