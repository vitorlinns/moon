using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moon.Api.Contracts.AdminAuth;
using Moon.Api.Contracts.Auth;
using Moon.Api.Data;
using Moon.Api.Domain;
using Moon.Api.Security;

namespace Moon.Api.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IAdminJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IOptions<AdminJwtOptions> jwtOptions,
    IWebHostEnvironment environment) : ControllerBase
{
    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutMinutes = 15;

    [EnableRateLimiting("admin-auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(AdminLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new ErrorResponse("Preencha e-mail e senha."));
        }

        await EnsureBootstrapAdminAsync(cancellationToken);

        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var admin = await dbContext.AdminUsers
            .FirstOrDefaultAsync(a => a.Email == emailNormalized, cancellationToken);

        if (admin is not null && admin.LockedUntil is { } lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
        {
            return StatusCode(StatusCodes.Status423Locked, new ErrorResponse(
                "Conta temporariamente bloqueada por muitas tentativas. Tente novamente em alguns minutos."));
        }

        // sempre roda o Verify (mesmo sem admin, contra um hash inválido) pra não vazar
        // por timing se o e-mail existe ou não
        var passwordHash = admin?.PasswordHash ?? passwordHasher.DummyHash;
        var passwordIsValid = passwordHasher.Verify(request.Password, passwordHash);

        if (admin is null || !admin.IsActive || !passwordIsValid)
        {
            if (admin is not null && admin.IsActive)
            {
                admin.FailedLoginAttempts++;
                if (admin.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    admin.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(LockoutMinutes);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return Unauthorized(new ErrorResponse("E-mail ou senha inválidos."));
        }

        admin.FailedLoginAttempts = 0;
        admin.LockedUntil = null;

        SetAccessTokenCookie(jwtTokenService.GenerateToken(admin));
        IssueRefreshToken(admin.Id);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(admin));
    }

    [EnableRateLimiting("admin-auth")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(AdminAuthCookie.RefreshToken, out var rawToken) || string.IsNullOrEmpty(rawToken))
        {
            return Unauthorized(new ErrorResponse("Sessão expirada. Faça login novamente."));
        }

        var tokenHash = refreshTokenService.Hash(rawToken);

        // Reivindica o token atomicamente (só marca como revogado se ainda não estiver),
        // pra que duas requisições concorrentes com o mesmo token nunca rotacionem as duas
        // a partir do mesmo pai — só uma "ganha" a corrida.
        var claimedCount = await dbContext.AdminRefreshTokens
            .Where(t => t.TokenHash == tokenHash && t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, DateTimeOffset.UtcNow), cancellationToken);

        var storedToken = await dbContext.AdminRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Unauthorized(new ErrorResponse("Sessão expirada. Faça login novamente."));
        }

        if (claimedCount == 0)
        {
            // não fomos nós que revogamos: token já tinha sido rotacionado antes (reuso —
            // possível roubo). derruba todas as sessões ativas do admin como precaução.
            await RevokeAllRefreshTokensAsync(storedToken.AdminUserId, cancellationToken);
            return Unauthorized(new ErrorResponse("Sessão inválida. Faça login novamente."));
        }

        var admin = await dbContext.AdminUsers.FindAsync([storedToken.AdminUserId], cancellationToken);
        if (admin is null || !admin.IsActive)
        {
            return Unauthorized();
        }

        SetAccessTokenCookie(jwtTokenService.GenerateToken(admin));
        IssueRefreshToken(admin.Id, replacing: storedToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [EnableRateLimiting("admin-auth")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(AdminAuthCookie.RefreshToken, out var rawToken) && !string.IsNullOrEmpty(rawToken))
        {
            var tokenHash = refreshTokenService.Hash(rawToken);
            var storedToken = await dbContext.AdminRefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

            if (storedToken is not null && storedToken.RevokedAt is null)
            {
                storedToken.RevokedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        Response.Cookies.Delete(AdminAuthCookie.AccessToken, new CookieOptions { Path = "/" });
        Response.Cookies.Delete(AdminAuthCookie.RefreshToken, new CookieOptions { Path = "/api/admin/auth" });

        return NoContent();
    }

    [Authorize(AuthenticationSchemes = AdminAuthCookie.BearerScheme)]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var admin = await GetCurrentAdminAsync(cancellationToken);
        if (admin is null)
        {
            return Unauthorized();
        }

        return Ok(ToResponse(admin));
    }

    private async Task<AdminUser?> GetCurrentAdminAsync(CancellationToken cancellationToken)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(adminIdClaim, out var adminId))
        {
            return null;
        }

        return await dbContext.AdminUsers.FindAsync([adminId], cancellationToken);
    }

    private void SetAccessTokenCookie(string token)
    {
        Response.Cookies.Append(AdminAuthCookie.AccessToken, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpiresMinutes),
            Path = "/",
        });
    }

    private void IssueRefreshToken(Guid adminUserId, AdminRefreshToken? replacing = null)
    {
        var rawToken = refreshTokenService.GenerateRawToken();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiresDays);

        var refreshToken = new AdminRefreshToken
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            TokenHash = refreshTokenService.Hash(rawToken),
            ExpiresAt = expiresAt,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
        };

        dbContext.AdminRefreshTokens.Add(refreshToken);

        if (replacing is not null)
        {
            replacing.RevokedAt = DateTimeOffset.UtcNow;
            replacing.ReplacedByTokenId = refreshToken.Id;
        }

        Response.Cookies.Append(AdminAuthCookie.RefreshToken, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt,
            Path = "/api/admin/auth",
        });
    }

    private async Task RevokeAllRefreshTokensAsync(Guid adminUserId, CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.AdminRefreshTokens
            .Where(t => t.AdminUserId == adminUserId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Se ainda não existe nenhum admin, cria o primeiro a partir de ADMIN_BOOTSTRAP_EMAIL/
    // ADMIN_BOOTSTRAP_PASSWORD do .env — não há autocadastro de admin, só outro admin cria
    // conta (feature futura), então precisa de algum jeito de nascer o primeiro. Roda sob
    // demanda (no login) em vez de no startup da API pra não depender de a migration já ter
    // sido aplicada nesse momento (nos testes, por exemplo, a migration só roda depois que a
    // fábrica de testes sobe a API pela primeira vez).
    private async Task EnsureBootstrapAdminAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.AdminUsers.AnyAsync(cancellationToken))
        {
            return;
        }

        var email = Environment.GetEnvironmentVariable("ADMIN_BOOTSTRAP_EMAIL");
        var password = Environment.GetEnvironmentVariable("ADMIN_BOOTSTRAP_PASSWORD");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        dbContext.AdminUsers.Add(new AdminUser
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(password),
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static AdminUserResponse ToResponse(AdminUser admin) => new(admin.Id, admin.Name, admin.Email);
}
