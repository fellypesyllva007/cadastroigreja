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
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions, JwtBearerHandler>("Bearer", null);
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (UnauthorizedAccessException ex)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync(ex.Message);
    }
});
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
users.MapPost("/{id:guid}/approve", async Task<Results<NoContent, NotFound>> (Guid id, ClaimsPrincipal principal, UserService service, CancellationToken ct) =>
    await service.ApproveAsync(id, CurrentUserId(principal), ct) ? TypedResults.NoContent() : TypedResults.NotFound());
users.MapPost("/{id:guid}/reject", async Task<Results<NoContent, NotFound>> (Guid id, ClaimsPrincipal principal, UserService service, CancellationToken ct) =>
    await service.RejectAsync(id, CurrentUserId(principal), ct) ? TypedResults.NoContent() : TypedResults.NotFound());


var roleRequests = app.MapGroup("/api/role-requests").WithTags("RoleRequests").RequireAuthorization();
roleRequests.MapGet("/", async (Guid? userId, RequestStatus? status, RoleChangeRequestService service, CancellationToken ct) =>
    TypedResults.Ok(await service.ListAsync(userId, status, ct)));
roleRequests.MapPost("/", async Task<Results<Created, BadRequest<string>>> (CreateRoleChangeRequest request, RoleChangeRequestService service, CancellationToken ct) =>
{
    try
    {
        var id = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/role-requests/{id}");
    }
    catch (InvalidOperationException ex)
    {
        return TypedResults.BadRequest(ex.Message);
    }
});
roleRequests.MapPost("/{id:guid}/approve", async Task<Results<NoContent, NotFound>> (Guid id, ClaimsPrincipal principal, RoleChangeRequestService service, CancellationToken ct) =>
    await service.ApproveAsync(id, CurrentUserId(principal), ct) ? TypedResults.NoContent() : TypedResults.NotFound());
roleRequests.MapPost("/{id:guid}/reject", async Task<Results<NoContent, NotFound>> (Guid id, ClaimsPrincipal principal, RoleChangeRequestService service, CancellationToken ct) =>
    await service.RejectAsync(id, CurrentUserId(principal), ct) ? TypedResults.NoContent() : TypedResults.NotFound());

var preacherRequests = app.MapGroup("/api/preacher-requests").WithTags("PreacherRequests").RequireAuthorization();
preacherRequests.MapGet("/", async (Guid? userId, RequestStatus? status, PreacherRequestService service, CancellationToken ct) =>
    TypedResults.Ok(await service.ListAsync(userId, status, ct)));
preacherRequests.MapPost("/", async Task<Results<Created, BadRequest<string>>> (CreatePreacherRequest request, PreacherRequestService service, CancellationToken ct) =>
{
    try
    {
        var id = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/preacher-requests/{id}");
    }
    catch (InvalidOperationException ex)
    {
        return TypedResults.BadRequest(ex.Message);
    }
});
preacherRequests.MapPost("/{id:guid}/approve", async Task<Results<Ok<PreacherRequestResponse>, NotFound>> (Guid id, ClaimsPrincipal principal, PreacherRequestService service, CancellationToken ct) =>
{
    var response = await service.ApproveAsync(id, CurrentUserId(principal), ct);
    return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
});
preacherRequests.MapPost("/{id:guid}/reject", async Task<Results<NoContent, NotFound>> (Guid id, ClaimsPrincipal principal, PreacherRequestService service, CancellationToken ct) =>
    await service.RejectAsync(id, CurrentUserId(principal), ct) ? TypedResults.NoContent() : TypedResults.NotFound());

var letters = app.MapGroup("/api/letters").WithTags("Letters");
letters.MapGet("/", async (Guid? userId, PreachingLetterService service, CancellationToken ct) =>
    TypedResults.Ok(await service.ListAsync(userId, ct))).RequireAuthorization();
letters.MapGet("/{id:guid}/validate", async Task<Results<Ok<PreachingLetterValidationResponse>, NotFound>> (Guid id, PreachingLetterService service, CancellationToken ct) =>
{
    var letter = await service.ValidateAsync(id, ct);
    return letter is null ? TypedResults.NotFound() : TypedResults.Ok(letter);
}).AllowAnonymous();
letters.MapGet("/{id:guid}/pdf", async Task<Results<FileContentHttpResult, NotFound>> (Guid id, PreachingLetterService service, CancellationToken ct) =>
{
    var pdf = await service.GetPdfAsync(id, ct);
    return pdf is null ? TypedResults.NotFound() : TypedResults.File(pdf, "application/pdf", $"carta-{id}.pdf");
}).RequireAuthorization();
letters.MapPost("/{id:guid}/suspend", async Task<Results<Ok<PreachingLetterResponse>, NotFound>> (Guid id, ClaimsPrincipal principal, PreachingLetterService service, CancellationToken ct) =>
{
    var letter = await service.SuspendAsync(id, CurrentUserId(principal), ct);
    return letter is null ? TypedResults.NotFound() : TypedResults.Ok(letter);
}).RequireAuthorization();
letters.MapPost("/{id:guid}/renew", async Task<Results<Ok<PreachingLetterResponse>, NotFound>> (Guid id, ClaimsPrincipal principal, PreachingLetterService service, CancellationToken ct) =>
{
    var letter = await service.RenewAsync(id, CurrentUserId(principal), ct);
    return letter is null ? TypedResults.NotFound() : TypedResults.Ok(letter);
}).RequireAuthorization();


var leaderSignatures = app.MapGroup("/api/leaders/signature").WithTags("LeaderSignatures").RequireAuthorization();
leaderSignatures.MapGet("/", async Task<Results<Ok<LeaderSignatureResponse>, NotFound>> (ClaimsPrincipal principal, LeaderSignatureService service, CancellationToken ct) =>
{
    var response = await service.GetAsync(CurrentUserId(principal)!.Value, ct);
    return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
});
leaderSignatures.MapPost("/", async Task<Results<Ok<LeaderSignatureResponse>, BadRequest<string>>> (LeaderSignatureRequest request, ClaimsPrincipal principal, LeaderSignatureService service, CancellationToken ct) =>
{
    try { return TypedResults.Ok(await service.SaveAsync(CurrentUserId(principal)!.Value, request, ct)); }
    catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
});
leaderSignatures.MapPut("/", async Task<Results<Ok<LeaderSignatureResponse>, BadRequest<string>>> (LeaderSignatureRequest request, ClaimsPrincipal principal, LeaderSignatureService service, CancellationToken ct) =>
{
    try { return TypedResults.Ok(await service.SaveAsync(CurrentUserId(principal)!.Value, request, ct)); }
    catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
});

var auditLogs = app.MapGroup("/api/audit-logs").WithTags("Audit").RequireAuthorization();
auditLogs.MapGet("/", async (string? entityName, string? entityId, ClaimsPrincipal principal, AuditLogService service, CancellationToken ct) =>
    TypedResults.Ok(await service.ListAuthorizedAsync(CurrentUserId(principal), entityName, entityId, ct)));

app.Run();

static Guid? CurrentUserId(ClaimsPrincipal? principal) =>
    Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

internal sealed class JwtBearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return Task.FromResult(AuthenticateResult.NoResult());

        var token = header[7..].Trim();
        if (!HmacJwtTokenService.TryValidate(token, configuration, out var principal)) return Task.FromResult(AuthenticateResult.Fail("Token inválido ou expirado."));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, principal.UserId.ToString()),
            new Claim(ClaimTypes.Email, principal.Email)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
