using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moon.Api.Contracts.Auth;
using Moon.Api.Data;
using Moon.Api.Security;

LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

// O .env usa nomes com underscore simples (CONNECTION_STRING_DEFAULT, JWT_KEY, ...);
// aqui traduzimos pra estrutura aninhada (Seção:Chave) que o resto do app espera.
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["ConnectionStrings:Default"] = Environment.GetEnvironmentVariable("CONNECTION_STRING_DEFAULT"),
    ["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY"),
    ["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    ["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
    ["Jwt:AccessTokenExpiresMinutes"] = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRES_MINUTES"),
    ["Jwt:RefreshTokenExpiresDays"] = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRES_DAYS"),
    ["Frontend:Origin"] = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN"),
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var frontendOrigin = builder.Configuration["Frontend:Origin"]
    ?? throw new InvalidOperationException("Configuração 'Frontend:Origin' ausente (defina FRONTEND_ORIGIN no .env).");

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Configuração 'Jwt' ausente (defina JWT_KEY, JWT_ISSUER e JWT_AUDIENCE no .env).");

if (jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException(
        "A chave 'Jwt:Key' precisa ter pelo menos 32 caracteres. Em produção, defina-a via variável de " +
        "ambiente (JWT_KEY) ou secret manager — nunca commitada em appsettings.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(AuthCookie.AccessToken, out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "moon_csrf";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var authPermitLimit = builder.Configuration.GetValue("RateLimit:AuthPermitLimit", 5);
var authWindowSeconds = builder.Configuration.GetValue("RateLimit:AuthWindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authPermitLimit,
            Window = TimeSpan.FromSeconds(authWindowSeconds),
            QueueLimit = 0,
        });
    });

    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
            new ErrorResponse("Muitas tentativas. Aguarde um instante e tente novamente."),
            cancellationToken));
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature is not null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionFeature.Error, "Erro não tratado ao processar {Path}", context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ErrorResponse("Erro interno. Tente novamente."));
    });
});

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Precisa vir depois de UseAuthentication: a validação do antiforgery compara o token
// com o usuário autenticado em context.User, que só é preenchido a partir daqui.
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var isStateChanging = HttpMethods.IsPost(method) || HttpMethods.IsPut(method)
        || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    if (isStateChanging && context.Request.Path.StartsWithSegments("/api"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException ex)
        {
            context.RequestServices.GetRequiredService<ILogger<Program>>()
                .LogWarning(ex, "Falha na validação do CSRF em {Path}", context.Request.Path);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ErrorResponse("Token de segurança ausente ou inválido."));
            return;
        }
    }

    await next(context);
});

app.MapControllers();

app.Run();

// Procura um .env subindo a partir do diretório atual e carrega suas variáveis no processo,
// sem sobrescrever variáveis já definidas (ex.: por um orquestrador em produção).
static void LoadDotEnv()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (directory is not null)
    {
        var envPath = Path.Combine(directory.FullName, ".env");
        if (File.Exists(envPath))
        {
            foreach (var line in File.ReadAllLines(envPath))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                {
                    continue;
                }

                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = trimmed[..separatorIndex].Trim();
                var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');

                if (Environment.GetEnvironmentVariable(key) is null)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }

            return;
        }

        directory = directory.Parent;
    }
}

// Torna a classe Program (gerada pelos top-level statements) acessível ao projeto de testes,
// que precisa dela pra WebApplicationFactory<Program>.
public partial class Program;
