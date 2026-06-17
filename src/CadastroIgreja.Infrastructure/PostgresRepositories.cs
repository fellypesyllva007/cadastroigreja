using System.Data;
using CadastroIgreja.Application;
using CadastroIgreja.Domain;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CadastroIgreja.Infrastructure;

public sealed class PostgresConnectionFactory(IConfiguration configuration)
{
    public string ConnectionString { get; } = configuration.GetConnectionString("Postgres")
        ?? Environment.GetEnvironmentVariable("CADASTROIGREJA_POSTGRES")
        ?? "Host=localhost;Port=5432;Database=cadastroigreja;Username=cadastroigreja;Password=cadastroigreja_dev";

    public async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

internal static class PgMapper
{
    public static Church ToChurch(IDataRecord r) => new()
    {
        Id = r.GetGuid(r.GetOrdinal("id")),
        Name = r.GetString(r.GetOrdinal("name")),
        Type = Enum.Parse<ChurchType>(r.GetString(r.GetOrdinal("type"))),
        ParentId = r.IsDBNull(r.GetOrdinal("parent_id")) ? null : r.GetGuid(r.GetOrdinal("parent_id")),
        Address = GetString(r, "address"), City = GetString(r, "city"), State = GetString(r, "state"),
        Phone = GetString(r, "phone"), Cnpj = GetString(r, "cnpj"), InstitutionalInfo = GetString(r, "institutional_info")
    };

    public static User ToUser(IDataRecord r) => new()
    {
        Id = r.GetGuid(r.GetOrdinal("id")), FullName = r.GetString(r.GetOrdinal("full_name")), Email = r.GetString(r.GetOrdinal("email")),
        Phone = GetString(r, "phone"), PasswordHash = r.GetString(r.GetOrdinal("password_hash")), ChurchId = r.GetGuid(r.GetOrdinal("church_id")),
        Role = Enum.Parse<MemberRole>(r.GetString(r.GetOrdinal("role_name"))), Status = MapUserStatus(r.GetString(r.GetOrdinal("status"))),
        ChurchJoinedAt = GetDateOnly(r, "church_joined_at")
    };

    public static RoleChangeRequest ToRoleRequest(IDataRecord r) => new()
    {
        Id = r.GetGuid(r.GetOrdinal("id")), UserId = r.GetGuid(r.GetOrdinal("user_id")),
        RequestedRole = Enum.Parse<MemberRole>(r.GetString(r.GetOrdinal("requested_role_name"))),
        Status = Enum.Parse<RequestStatus>(r.GetString(r.GetOrdinal("status"))), Justification = GetString(r, "justification"),
        CreatedAt = r.GetFieldValue<DateTimeOffset>(r.GetOrdinal("created_at")), DecidedAt = GetDateTimeOffset(r, "decided_at")
    };

    public static PreacherRequest ToPreacherRequest(IDataRecord r) => new()
    {
        Id = r.GetGuid(r.GetOrdinal("id")), UserId = r.GetGuid(r.GetOrdinal("user_id")), ChurchId = r.GetGuid(r.GetOrdinal("origin_church_id")),
        Status = Enum.Parse<RequestStatus>(r.GetString(r.GetOrdinal("status"))), CurrentStep = Enum.Parse<PreacherApprovalStep>(r.GetString(r.GetOrdinal("current_step"))),
        Notes = GetString(r, "notes"), CreatedAt = r.GetFieldValue<DateTimeOffset>(r.GetOrdinal("created_at")), DecidedAt = GetDateTimeOffset(r, "decided_at"),
        LetterId = r.IsDBNull(r.GetOrdinal("letter_id")) ? null : r.GetGuid(r.GetOrdinal("letter_id")),
        DestinationChurchId = r.IsDBNull(r.GetOrdinal("destination_church_id")) ? null : r.GetGuid(r.GetOrdinal("destination_church_id"))
    };

    public static PreachingLetter ToLetter(IDataRecord r) => new()
    {
        Id = r.GetGuid(r.GetOrdinal("id")), LetterNumber = r.GetString(r.GetOrdinal("number")), PreacherRequestId = r.GetGuid(r.GetOrdinal("preacher_request_id")),
        UserId = r.GetGuid(r.GetOrdinal("user_id")), ChurchId = r.GetGuid(r.GetOrdinal("church_id")), DestinationChurchId = r.IsDBNull(r.GetOrdinal("destination_church_id")) ? null : r.GetGuid(r.GetOrdinal("destination_church_id")),
        ApprovedByUserId = r.GetGuid(r.GetOrdinal("issued_by")), ApprovedAt = r.GetFieldValue<DateTimeOffset>(r.GetOrdinal("approved_at")),
        PdfStoragePath = r.GetString(r.GetOrdinal("pdf_path")), QrCodeValue = r.GetString(r.GetOrdinal("qr_code_payload")), Status = Enum.Parse<LetterStatus>(r.GetString(r.GetOrdinal("status"))),
        CreatedAt = r.GetFieldValue<DateTimeOffset>(r.GetOrdinal("created_at")), ValidUntil = DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("expiration_date")))
    };

    public static LeaderSignature ToSignature(IDataRecord r) => new() { Id = r.GetGuid(r.GetOrdinal("id")), LeaderId = r.GetGuid(r.GetOrdinal("leader_id")), StoragePath = r.GetString(r.GetOrdinal("storage_path")), MimeType = r.GetString(r.GetOrdinal("mime_type")), CreatedAt = r.GetFieldValue<DateTimeOffset>(r.GetOrdinal("created_at")), UpdatedAt = r.GetFieldValue<DateTimeOffset>(r.GetOrdinal("updated_at")), Active = r.GetBoolean(r.GetOrdinal("active")) };
    public static AuditLog ToAuditLog(IDataRecord r) => new() { Id = r.GetInt64(r.GetOrdinal("id")), UserId = r.IsDBNull(r.GetOrdinal("user_id")) ? null : r.GetGuid(r.GetOrdinal("user_id")), Action = r.GetString(r.GetOrdinal("action")), EntityName = r.GetString(r.GetOrdinal("entity_name")), EntityId = r.GetString(r.GetOrdinal("entity_id")), Metadata = GetString(r, "metadata"), CreatedAt = r.GetFieldValue<DateTimeOffset>(r.GetOrdinal("created_at")) };
    public static string DbUserStatus(UserStatus s) => s == UserStatus.Approved ? "Approved" : s.ToString();
    private static UserStatus MapUserStatus(string value) => value == "Active" ? UserStatus.Approved : Enum.Parse<UserStatus>(value);
    private static string? GetString(IDataRecord r, string name) { var i = r.GetOrdinal(name); return r.IsDBNull(i) ? null : r.GetValue(i).ToString(); }
    private static DateOnly? GetDateOnly(IDataRecord r, string name) { var i = r.GetOrdinal(name); return r.IsDBNull(i) ? null : DateOnly.FromDateTime(r.GetDateTime(i)); }
    private static DateTimeOffset? GetDateTimeOffset(IDataRecord r, string name) { var i = r.GetOrdinal(name); return r.IsDBNull(i) ? null : r.GetFieldValue<DateTimeOffset>(i); }
}

public sealed class PostgresChurchRepository(PostgresConnectionFactory db) : IChurchRepository
{
    public async Task AddAsync(Church c, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("INSERT INTO churches (id,name,type,parent_id,address,city,state,phone,cnpj,institutional_info) VALUES (@id,@n,@t,@p,@a,@c,@s,@ph,@cnpj,@ii)", cn); cmd.Parameters.AddWithValue("id", c.Id); cmd.Parameters.AddWithValue("n", c.Name); cmd.Parameters.AddWithValue("t", c.Type.ToString()); cmd.Parameters.AddWithValue("p", (object?)c.ParentId ?? DBNull.Value); cmd.Parameters.AddWithValue("a", (object?)c.Address ?? DBNull.Value); cmd.Parameters.AddWithValue("c", (object?)c.City ?? DBNull.Value); cmd.Parameters.AddWithValue("s", (object?)c.State ?? DBNull.Value); cmd.Parameters.AddWithValue("ph", (object?)c.Phone ?? DBNull.Value); cmd.Parameters.AddWithValue("cnpj", (object?)c.Cnpj ?? DBNull.Value); cmd.Parameters.AddWithValue("ii", (object?)c.InstitutionalInfo ?? DBNull.Value); await cmd.ExecuteNonQueryAsync(ct); }
    public async Task<Church?> GetByIdAsync(Guid id, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT * FROM churches WHERE id=@id", cn); cmd.Parameters.AddWithValue("id", id); await using var r = await cmd.ExecuteReaderAsync(ct); return await r.ReadAsync(ct) ? PgMapper.ToChurch(r) : null; }
    public async Task<IReadOnlyCollection<Church>> ListAsync(Guid? parentId, ChurchType? type, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT * FROM churches WHERE (@p IS NULL OR parent_id=@p) AND (@t IS NULL OR type=@t) ORDER BY name", cn); cmd.Parameters.AddWithValue("p", (object?)parentId ?? DBNull.Value); cmd.Parameters.AddWithValue("t", (object?)type?.ToString() ?? DBNull.Value); var list = new List<Church>(); await using var r = await cmd.ExecuteReaderAsync(ct); while (await r.ReadAsync(ct)) list.Add(PgMapper.ToChurch(r)); return list; }
}

public sealed class PostgresUserRepository(PostgresConnectionFactory db) : IUserRepository
{
    private readonly Dictionary<Guid, User> _tracked = new();
    public async Task AddAsync(User u, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("INSERT INTO users (id,full_name,email,phone,password_hash,church_id,role_id,status,church_joined_at) VALUES (@id,@n,@e,@p,@h,@c,(SELECT id FROM roles WHERE name=@r),@s,@j)", cn); cmd.Parameters.AddWithValue("id", u.Id); cmd.Parameters.AddWithValue("n", u.FullName); cmd.Parameters.AddWithValue("e", u.Email); cmd.Parameters.AddWithValue("p", (object?)u.Phone ?? DBNull.Value); cmd.Parameters.AddWithValue("h", u.PasswordHash); cmd.Parameters.AddWithValue("c", u.ChurchId); cmd.Parameters.AddWithValue("r", u.Role.ToString()); cmd.Parameters.AddWithValue("s", PgMapper.DbUserStatus(u.Status)); cmd.Parameters.AddWithValue("j", (object?)u.ChurchJoinedAt ?? DBNull.Value); await cmd.ExecuteNonQueryAsync(ct); _tracked[u.Id] = u; }
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => GetAsync("u.email=lower(@v)", email.ToLowerInvariant(), ct);
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => GetAsync("u.id=@v", id, ct);
    private async Task<User?> GetAsync(string where, object value, CancellationToken ct) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand($"SELECT u.*, r.name role_name FROM users u JOIN roles r ON r.id=u.role_id WHERE {where}", cn); cmd.Parameters.AddWithValue("v", value); await using var rd = await cmd.ExecuteReaderAsync(ct); if (!await rd.ReadAsync(ct)) return null; var u = PgMapper.ToUser(rd); _tracked[u.Id] = u; return u; }
    public async Task SaveAsync(CancellationToken cancellationToken = default) { await using var cn = await db.OpenAsync(cancellationToken); foreach (var u in _tracked.Values) { await using var cmd = new NpgsqlCommand("UPDATE users SET full_name=@n,email=@e,phone=@p,password_hash=@h,church_id=@c,role_id=(SELECT id FROM roles WHERE name=@r),status=@s,church_joined_at=@j WHERE id=@id", cn); cmd.Parameters.AddWithValue("id", u.Id); cmd.Parameters.AddWithValue("n", u.FullName); cmd.Parameters.AddWithValue("e", u.Email); cmd.Parameters.AddWithValue("p", (object?)u.Phone ?? DBNull.Value); cmd.Parameters.AddWithValue("h", u.PasswordHash); cmd.Parameters.AddWithValue("c", u.ChurchId); cmd.Parameters.AddWithValue("r", u.Role.ToString()); cmd.Parameters.AddWithValue("s", PgMapper.DbUserStatus(u.Status)); cmd.Parameters.AddWithValue("j", (object?)u.ChurchJoinedAt ?? DBNull.Value); await cmd.ExecuteNonQueryAsync(cancellationToken); } }
}

public sealed class PostgresRoleChangeRequestRepository(PostgresConnectionFactory db) : IRoleChangeRequestRepository
{
    private readonly Dictionary<Guid, RoleChangeRequest> _tracked = new();
    public async Task AddAsync(RoleChangeRequest r, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("INSERT INTO role_change_requests (id,user_id,requested_role_id,status,justification,created_at,decided_at) VALUES (@id,@u,(SELECT id FROM roles WHERE name=@role),@s,@j,@c,@d)", cn); cmd.Parameters.AddWithValue("id", r.Id); cmd.Parameters.AddWithValue("u", r.UserId); cmd.Parameters.AddWithValue("role", r.RequestedRole.ToString()); cmd.Parameters.AddWithValue("s", r.Status.ToString()); cmd.Parameters.AddWithValue("j", (object?)r.Justification ?? DBNull.Value); cmd.Parameters.AddWithValue("c", r.CreatedAt); cmd.Parameters.AddWithValue("d", (object?)r.DecidedAt ?? DBNull.Value); await cmd.ExecuteNonQueryAsync(ct); _tracked[r.Id] = r; }
    public async Task<RoleChangeRequest?> GetByIdAsync(Guid id, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT r.*, roles.name requested_role_name FROM role_change_requests r JOIN roles ON roles.id=r.requested_role_id WHERE r.id=@id", cn); cmd.Parameters.AddWithValue("id", id); await using var rd = await cmd.ExecuteReaderAsync(ct); if (!await rd.ReadAsync(ct)) return null; var entity = PgMapper.ToRoleRequest(rd); _tracked[entity.Id] = entity; return entity; }
    public async Task<IReadOnlyCollection<RoleChangeRequest>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT r.*, roles.name requested_role_name FROM role_change_requests r JOIN roles ON roles.id=r.requested_role_id WHERE (@u IS NULL OR user_id=@u) AND (@s IS NULL OR status=@s) ORDER BY created_at DESC", cn); cmd.Parameters.AddWithValue("u", (object?)userId ?? DBNull.Value); cmd.Parameters.AddWithValue("s", (object?)status?.ToString() ?? DBNull.Value); var list = new List<RoleChangeRequest>(); await using var rd = await cmd.ExecuteReaderAsync(ct); while (await rd.ReadAsync(ct)) { var e = PgMapper.ToRoleRequest(rd); _tracked[e.Id] = e; list.Add(e); } return list; }
    public async Task SaveAsync(CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); foreach (var r in _tracked.Values) { await using var cmd = new NpgsqlCommand("UPDATE role_change_requests SET status=@s, decided_at=@d, requested_role_id=(SELECT id FROM roles WHERE name=@role), justification=@j WHERE id=@id", cn); cmd.Parameters.AddWithValue("id", r.Id); cmd.Parameters.AddWithValue("s", r.Status.ToString()); cmd.Parameters.AddWithValue("d", (object?)r.DecidedAt ?? DBNull.Value); cmd.Parameters.AddWithValue("role", r.RequestedRole.ToString()); cmd.Parameters.AddWithValue("j", (object?)r.Justification ?? DBNull.Value); await cmd.ExecuteNonQueryAsync(ct); } }
}

public sealed class PostgresPreacherRequestRepository(PostgresConnectionFactory db) : IPreacherRequestRepository
{
    private readonly Dictionary<Guid, PreacherRequest> _tracked = new();
    public async Task AddAsync(PreacherRequest r, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("INSERT INTO preacher_requests (id,user_id,origin_church_id,destination_church_id,status,current_step,notes,created_at,decided_at,letter_id) VALUES (@id,@u,@c,@d,@s,@st,@n,@cr,@de,@l)", cn); Add(cmd, r); await cmd.ExecuteNonQueryAsync(ct); _tracked[r.Id] = r; }
    public async Task<PreacherRequest?> GetByIdAsync(Guid id, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT * FROM preacher_requests WHERE id=@id", cn); cmd.Parameters.AddWithValue("id", id); await using var rd = await cmd.ExecuteReaderAsync(ct); if (!await rd.ReadAsync(ct)) return null; var e = PgMapper.ToPreacherRequest(rd); _tracked[e.Id] = e; return e; }
    public async Task<IReadOnlyCollection<PreacherRequest>> ListAsync(Guid? userId, RequestStatus? status, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT * FROM preacher_requests WHERE (@u IS NULL OR user_id=@u) AND (@s IS NULL OR status=@s) ORDER BY created_at DESC", cn); cmd.Parameters.AddWithValue("u", (object?)userId ?? DBNull.Value); cmd.Parameters.AddWithValue("s", (object?)status?.ToString() ?? DBNull.Value); var list = new List<PreacherRequest>(); await using var rd = await cmd.ExecuteReaderAsync(ct); while (await rd.ReadAsync(ct)) { var e = PgMapper.ToPreacherRequest(rd); _tracked[e.Id] = e; list.Add(e); } return list; }
    public async Task SaveAsync(CancellationToken cancellationToken = default) { await using var cn = await db.OpenAsync(cancellationToken); foreach (var r in _tracked.Values) { await using var cmd = new NpgsqlCommand("UPDATE preacher_requests SET status=@s,current_step=@st,decided_at=@d,letter_id=@l WHERE id=@id", cn); cmd.Parameters.AddWithValue("id", r.Id); cmd.Parameters.AddWithValue("s", r.Status.ToString()); cmd.Parameters.AddWithValue("st", r.CurrentStep.ToString()); cmd.Parameters.AddWithValue("d", (object?)r.DecidedAt ?? DBNull.Value); cmd.Parameters.AddWithValue("l", (object?)r.LetterId ?? DBNull.Value); await cmd.ExecuteNonQueryAsync(cancellationToken); } }
    private static void Add(NpgsqlCommand cmd, PreacherRequest r) { cmd.Parameters.AddWithValue("id", r.Id); cmd.Parameters.AddWithValue("u", r.UserId); cmd.Parameters.AddWithValue("c", r.ChurchId); cmd.Parameters.AddWithValue("d", (object?)r.DestinationChurchId ?? DBNull.Value); cmd.Parameters.AddWithValue("s", r.Status.ToString()); cmd.Parameters.AddWithValue("st", r.CurrentStep.ToString()); cmd.Parameters.AddWithValue("n", (object?)r.Notes ?? DBNull.Value); cmd.Parameters.AddWithValue("cr", r.CreatedAt); cmd.Parameters.AddWithValue("de", (object?)r.DecidedAt ?? DBNull.Value); cmd.Parameters.AddWithValue("l", (object?)r.LetterId ?? DBNull.Value); }
}

public sealed class PostgresPreachingLetterRepository(PostgresConnectionFactory db) : IPreachingLetterRepository
{
    private readonly Dictionary<Guid, PreachingLetter> _tracked = new();
    public async Task AddAsync(PreachingLetter l, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("INSERT INTO preaching_letters (id,preacher_request_id,user_id,church_id,destination_church_id,number,expiration_date,status,pdf_path,qr_code_payload,issued_by,approved_at,created_at) VALUES (@id,@r,@u,@c,@d,@n,@e,@s,@p,@q,@i,@a,@cr)", cn); cmd.Parameters.AddWithValue("id", l.Id); cmd.Parameters.AddWithValue("r", l.PreacherRequestId); cmd.Parameters.AddWithValue("u", l.UserId); cmd.Parameters.AddWithValue("c", l.ChurchId); cmd.Parameters.AddWithValue("d", (object?)l.DestinationChurchId ?? DBNull.Value); cmd.Parameters.AddWithValue("n", l.LetterNumber); cmd.Parameters.AddWithValue("e", l.ValidUntil); cmd.Parameters.AddWithValue("s", l.Status.ToString()); cmd.Parameters.AddWithValue("p", l.PdfStoragePath); cmd.Parameters.AddWithValue("q", l.QrCodeValue); cmd.Parameters.AddWithValue("i", l.ApprovedByUserId); cmd.Parameters.AddWithValue("a", l.ApprovedAt); cmd.Parameters.AddWithValue("cr", l.CreatedAt); await cmd.ExecuteNonQueryAsync(ct); _tracked[l.Id] = l; }
    public async Task<PreachingLetter?> GetByIdAsync(Guid id, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT * FROM preaching_letters WHERE id=@id", cn); cmd.Parameters.AddWithValue("id", id); await using var rd = await cmd.ExecuteReaderAsync(ct); if (!await rd.ReadAsync(ct)) return null; var e = PgMapper.ToLetter(rd); _tracked[e.Id] = e; return e; }
    public async Task<IReadOnlyCollection<PreachingLetter>> ListAsync(Guid? userId, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT * FROM preaching_letters WHERE (@u IS NULL OR user_id=@u) ORDER BY created_at DESC", cn); cmd.Parameters.AddWithValue("u", (object?)userId ?? DBNull.Value); var list = new List<PreachingLetter>(); await using var rd = await cmd.ExecuteReaderAsync(ct); while (await rd.ReadAsync(ct)) { var e = PgMapper.ToLetter(rd); _tracked[e.Id] = e; list.Add(e); } return list; }
    public async Task SaveAsync(CancellationToken cancellationToken = default) { await using var cn = await db.OpenAsync(cancellationToken); foreach (var l in _tracked.Values) { await using var cmd = new NpgsqlCommand("UPDATE preaching_letters SET status=@s, expiration_date=@e, pdf_path=@p, qr_code_payload=@q WHERE id=@id", cn); cmd.Parameters.AddWithValue("id", l.Id); cmd.Parameters.AddWithValue("s", l.Status.ToString()); cmd.Parameters.AddWithValue("e", l.ValidUntil); cmd.Parameters.AddWithValue("p", l.PdfStoragePath); cmd.Parameters.AddWithValue("q", l.QrCodeValue); await cmd.ExecuteNonQueryAsync(cancellationToken); } }
}

public sealed class PostgresAuditLogRepository(PostgresConnectionFactory db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog l, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("INSERT INTO audit_logs (user_id,action,entity_name,entity_id,metadata,created_at) VALUES (@u,@a,@en,@ei,CAST(@m AS jsonb),@c)", cn); cmd.Parameters.AddWithValue("u", (object?)l.UserId ?? DBNull.Value); cmd.Parameters.AddWithValue("a", l.Action); cmd.Parameters.AddWithValue("en", l.EntityName); cmd.Parameters.AddWithValue("ei", l.EntityId); cmd.Parameters.AddWithValue("m", string.IsNullOrWhiteSpace(l.Metadata) ? "{}" : System.Text.Json.JsonSerializer.Serialize(l.Metadata)); cmd.Parameters.AddWithValue("c", l.CreatedAt); await cmd.ExecuteNonQueryAsync(ct); }
    public async Task<IReadOnlyCollection<AuditLog>> ListAsync(string? entityName, string? entityId, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT id,user_id,action,entity_name,entity_id,metadata::text metadata,created_at FROM audit_logs WHERE (@n IS NULL OR entity_name=@n) AND (@i IS NULL OR entity_id=@i) ORDER BY created_at DESC", cn); cmd.Parameters.AddWithValue("n", (object?)entityName ?? DBNull.Value); cmd.Parameters.AddWithValue("i", (object?)entityId ?? DBNull.Value); var list = new List<AuditLog>(); await using var rd = await cmd.ExecuteReaderAsync(ct); while (await rd.ReadAsync(ct)) list.Add(PgMapper.ToAuditLog(rd)); return list; }
}

public sealed class PostgresLeaderSignatureRepository(PostgresConnectionFactory db) : ILeaderSignatureRepository
{
    private readonly Dictionary<Guid, LeaderSignature> _tracked = new();
    public async Task AddAsync(LeaderSignature s, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("INSERT INTO leader_signatures (id,leader_id,storage_path,mime_type,created_at,updated_at,active) VALUES (@id,@l,@p,@m,@c,@u,@a)", cn); cmd.Parameters.AddWithValue("id", s.Id); cmd.Parameters.AddWithValue("l", s.LeaderId); cmd.Parameters.AddWithValue("p", s.StoragePath); cmd.Parameters.AddWithValue("m", s.MimeType); cmd.Parameters.AddWithValue("c", s.CreatedAt); cmd.Parameters.AddWithValue("u", s.UpdatedAt); cmd.Parameters.AddWithValue("a", s.Active); await cmd.ExecuteNonQueryAsync(ct); _tracked[s.Id] = s; }
    public async Task<LeaderSignature?> GetActiveByLeaderIdAsync(Guid leaderId, CancellationToken ct = default) { var all = await ListByLeaderIdAsync(leaderId, ct); return all.FirstOrDefault(x => x.Active); }
    public async Task<IReadOnlyCollection<LeaderSignature>> ListByLeaderIdAsync(Guid leaderId, CancellationToken ct = default) { await using var cn = await db.OpenAsync(ct); await using var cmd = new NpgsqlCommand("SELECT * FROM leader_signatures WHERE leader_id=@id ORDER BY created_at DESC", cn); cmd.Parameters.AddWithValue("id", leaderId); var list = new List<LeaderSignature>(); await using var rd = await cmd.ExecuteReaderAsync(ct); while (await rd.ReadAsync(ct)) { var e = PgMapper.ToSignature(rd); _tracked[e.Id] = e; list.Add(e); } return list; }
    public async Task SaveAsync(CancellationToken cancellationToken = default) { await using var cn = await db.OpenAsync(cancellationToken); foreach (var s in _tracked.Values) { await using var cmd = new NpgsqlCommand("UPDATE leader_signatures SET active=@a, updated_at=@u WHERE id=@id", cn); cmd.Parameters.AddWithValue("id", s.Id); cmd.Parameters.AddWithValue("a", s.Active); cmd.Parameters.AddWithValue("u", s.UpdatedAt); await cmd.ExecuteNonQueryAsync(cancellationToken); } }
}
