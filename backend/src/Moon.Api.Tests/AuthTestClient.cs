using System.Net.Http.Json;
using System.Text.Json;

namespace Moon.Api.Tests;

/// <summary>
/// Cliente HTTP com controle manual de cookies (não usa o CookieContainer padrão), pra poder
/// clonar/"congelar" o estado de uma sessão e simular reuso de um cookie antigo — exatamente
/// o cenário de token roubado que o RefreshTokenTests precisa reproduzir.
/// </summary>
internal sealed class AuthTestClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly Dictionary<string, string> _cookies;

    public AuthTestClient(HttpClient http, Dictionary<string, string>? cookies = null)
    {
        _http = http;
        _cookies = cookies is null ? [] : new Dictionary<string, string>(cookies);
    }

    /// <summary>Cria um cliente independente com uma cópia congelada dos cookies atuais.</summary>
    public AuthTestClient Fork() => new(_http, _cookies);

    public string? GetCookie(string name) => _cookies.GetValueOrDefault(name);

    public async Task<HttpResponseMessage> GetAsync(string path)
    {
        var response = await SendRawAsync(HttpMethod.Get, path);
        CaptureCookies(response);
        return response;
    }

    public async Task<HttpResponseMessage> PostAsync(string path, object? body = null) =>
        await SendWithCsrfAsync(HttpMethod.Post, path, body);

    public async Task<HttpResponseMessage> PatchAsync(string path, object? body = null) =>
        await SendWithCsrfAsync(HttpMethod.Patch, path, body);

    public async Task<HttpResponseMessage> PutAsync(string path, object? body = null) =>
        await SendWithCsrfAsync(HttpMethod.Put, path, body);

    public async Task<HttpResponseMessage> DeleteAsync(string path, object? body = null) =>
        await SendWithCsrfAsync(HttpMethod.Delete, path, body);

    /// <summary>Envia sem token CSRF, mesmo sendo POST/PATCH — pra testar que a validação bloqueia.</summary>
    public async Task<HttpResponseMessage> PostWithoutCsrfAsync(string path, object? body = null)
    {
        var response = await SendRawAsync(HttpMethod.Post, path, body);
        CaptureCookies(response);
        return response;
    }

    private async Task<HttpResponseMessage> SendWithCsrfAsync(HttpMethod method, string path, object? body)
    {
        var csrfResponse = await SendRawAsync(HttpMethod.Get, "/api/auth/csrf-token");
        CaptureCookies(csrfResponse);
        var json = await csrfResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = json.GetProperty("token").GetString()!;

        var response = await SendRawAsync(method, path, body, token);
        CaptureCookies(response);
        return response;
    }

    private async Task<HttpResponseMessage> SendRawAsync(
        HttpMethod method, string path, object? body = null, string? csrfToken = null)
    {
        var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        if (_cookies.Count > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", _cookies.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        if (csrfToken is not null)
        {
            request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        }

        return await _http.SendAsync(request);
    }

    private void CaptureCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            return;
        }

        foreach (var header in setCookieHeaders)
        {
            var nameValue = header.Split(';', 2)[0];
            var separatorIndex = nameValue.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            _cookies[nameValue[..separatorIndex].Trim()] = nameValue[(separatorIndex + 1)..].Trim();
        }
    }
}
