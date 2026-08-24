using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moon.Api.Contracts.Auth;
using Moon.Api.Data;
using Moon.Api.Domain;
using Moon.Api.Security;

namespace Moon.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IAntiforgery antiforgery,
    IOptions<JwtOptions> jwtOptions,
    IWebHostEnvironment environment) : ControllerBase
{
    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutMinutes = 15;

    [HttpGet("csrf-token")]
    public IActionResult GetCsrfToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Cpf) || !CpfValidator.IsValid(request.Cpf))
        {
            return BadRequest(new ErrorResponse("Informe um CPF válido."));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ErrorResponse("Informe seu nome."));
        }

        if (!EmailValidator.IsValid(request.Email))
        {
            return BadRequest(new ErrorResponse("Informe um e-mail válido."));
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        {
            return BadRequest(new ErrorResponse("A senha precisa ter no mínimo 8 caracteres."));
        }

        var cpfDigits = new string(request.Cpf.Where(char.IsDigit).ToArray());

        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var alreadyExists = await dbContext.Users
            .AnyAsync(u => u.Cpf == cpfDigits || u.Email == emailNormalized, cancellationToken);

        if (alreadyExists)
        {
            return Conflict(new ErrorResponse("Já existe uma conta com esse CPF ou e-mail."));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = emailNormalized,
            Cpf = cpfDigits,
            PasswordHash = passwordHasher.Hash(request.Password),
        };

        dbContext.Users.Add(user);
        await SignInAsync(user, cancellationToken);

        return Ok(ToResponse(user));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new ErrorResponse("Preencha e-mail e senha."));
        }

        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == emailNormalized, cancellationToken);

        if (user is not null && user.LockedUntil is { } lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
        {
            return StatusCode(StatusCodes.Status423Locked, new ErrorResponse(
                "Conta temporariamente bloqueada por muitas tentativas. Tente novamente em alguns minutos."));
        }

        // sempre roda o Verify (mesmo sem usuário, contra um hash inválido) pra não vazar
        // por timing se o e-mail existe ou não
        var passwordHash = user?.PasswordHash ?? passwordHasher.DummyHash;
        var passwordIsValid = passwordHasher.Verify(request.Password, passwordHash);

        if (user?.PasswordHash is null || !passwordIsValid)
        {
            if (user is not null)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(LockoutMinutes);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return Unauthorized(new ErrorResponse("E-mail ou senha inválidos."));
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;

        await SignInAsync(user, cancellationToken);

        return Ok(ToResponse(user));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(AuthCookie.RefreshToken, out var rawToken) || string.IsNullOrEmpty(rawToken))
        {
            return Unauthorized(new ErrorResponse("Sessão expirada. Faça login novamente."));
        }

        var tokenHash = refreshTokenService.Hash(rawToken);

        // Reivindica o token atomicamente (só marca como revogado se ainda não estiver),
        // pra que duas requisições concorrentes com o mesmo token nunca rotacionem as duas
        // a partir do mesmo pai — só uma "ganha" a corrida.
        var claimedCount = await dbContext.RefreshTokens
            .Where(t => t.TokenHash == tokenHash && t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, DateTimeOffset.UtcNow), cancellationToken);

        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Unauthorized(new ErrorResponse("Sessão expirada. Faça login novamente."));
        }

        if (claimedCount == 0)
        {
            // não fomos nós que revogamos: token já tinha sido rotacionado antes (reuso —
            // possível roubo). derruba todas as sessões ativas do usuário como precaução.
            await RevokeAllRefreshTokensAsync(storedToken.UserId, cancellationToken);
            return Unauthorized(new ErrorResponse("Sessão inválida. Faça login novamente."));
        }

        var user = await dbContext.Users.FindAsync([storedToken.UserId], cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        SetAccessTokenCookie(jwtTokenService.GenerateToken(user));
        IssueRefreshToken(user.Id, replacing: storedToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(AuthCookie.RefreshToken, out var rawToken) && !string.IsNullOrEmpty(rawToken))
        {
            var tokenHash = refreshTokenService.Hash(rawToken);
            var storedToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

            if (storedToken is not null && storedToken.RevokedAt is null)
            {
                storedToken.RevokedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        Response.Cookies.Delete(AuthCookie.AccessToken, new CookieOptions { Path = "/" });
        Response.Cookies.Delete(AuthCookie.RefreshToken, new CookieOptions { Path = "/api/auth" });

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(ToResponse(user));
    }

    [Authorize]
    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ErrorResponse("Informe seu nome."));
        }

        if (!EmailValidator.IsValid(request.Email))
        {
            return BadRequest(new ErrorResponse("Informe um e-mail válido."));
        }

        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await dbContext.Users
            .AnyAsync(u => u.Email == emailNormalized && u.Id != user.Id, cancellationToken);

        if (emailTaken)
        {
            return Conflict(new ErrorResponse("Esse e-mail já está em uso."));
        }

        user.Name = request.Name.Trim();
        user.Email = emailNormalized;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(user));
    }

    private async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return await dbContext.Users.FindAsync([userId], cancellationToken);
    }

    private async Task SignInAsync(User user, CancellationToken cancellationToken)
    {
        SetAccessTokenCookie(jwtTokenService.GenerateToken(user));
        IssueRefreshToken(user.Id);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void SetAccessTokenCookie(string token)
    {
        Response.Cookies.Append(AuthCookie.AccessToken, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpiresMinutes),
            Path = "/",
        });
    }

    /// <summary>
    /// Cria um novo refresh token pro usuário e já grava o cookie. Se <paramref name="replacing"/>
    /// for informado, revoga o token antigo e liga os dois na cadeia de rotação.
    /// Não chama SaveChanges — quem chamar é responsável por persistir.
    /// </summary>
    private void IssueRefreshToken(Guid userId, RefreshToken? replacing = null)
    {
        var rawToken = refreshTokenService.GenerateRawToken();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiresDays);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = refreshTokenService.Hash(rawToken),
            ExpiresAt = expiresAt,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
        };

        dbContext.RefreshTokens.Add(refreshToken);

        if (replacing is not null)
        {
            replacing.RevokedAt = DateTimeOffset.UtcNow;
            replacing.ReplacedByTokenId = refreshToken.Id;
        }

        Response.Cookies.Append(AuthCookie.RefreshToken, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt,
            Path = "/api/auth",
        });
    }

    private async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email, user.Cpf);
}
