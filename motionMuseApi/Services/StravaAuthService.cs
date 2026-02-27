using System.Net.Http.Headers;
using System.Web;
using motionMuseApi.Models;
using motionMuseApi.Repositories;

namespace motionMuseApi.Services
{
  public class StravaAuthService(IStravaLinkRepository stravaLinkRepository, IHttpClientFactory httpClientFactory) : IStravaAuthService
  {
    private readonly HttpClient _client = httpClientFactory.CreateClient("auth0Client");

    public async Task<string?> GetStravaValidAccessTokenAsync(string auth0UserId, CancellationToken ct = default)
    {
      var entity = await stravaLinkRepository.GetByAuth0UserIdAsync(auth0UserId, ct);

      string? stravaAccessToken = entity?.AccessToken;

      if (entity?.AccessToken is null)
      {
        // Query the Oauth via M2M to get the correct token
        // var m2mApiToken = await GetManagementApiToken(ct) ?? throw new Exception("No token response from M2M");

        // var userFromOAuth = await service.GetUserFromOAuth(auth0UserId, m2mApiToken) ?? throw new Exception("No user found on Oauth");
        // stravaAccessToken = userFromOAuth.Identities.FirstOrDefault()?.AccessToken;
      }

      // Je query la BD pour lire la validité du token sur strava.
      // SI il est périmé j'en demande un autre via le refresh token

      return stravaAccessToken;
    }

    async Task<string?> GetManagementApiToken(CancellationToken ct = default)
    {
      // Query the Oauth via M2M to get the correct token
      var tokenResponse = await GetAccessTokenFromM2M(ct);

      return tokenResponse?.AccessToken ?? null;
    }

    static string DecodeJwtPayload(string jwt)
    {
      var parts = jwt.Split('.');
      if (parts.Length < 2) return "<not a jwt>";
      string Pad(string s) => s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=').Replace('-', '+').Replace('_', '/');
      var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
      return json;
    }


    public async Task<UserResponseDto?> GetUserFromOAuth(string auth0UserId, string token, CancellationToken ct = default)
    {
      var encodedAuth0UserId = Uri.EscapeDataString(auth0UserId);
      var URI = $"/api/v2/users/{encodedAuth0UserId}";

      var accessToken = token.Trim().Trim('"');

      Console.WriteLine("JWT payload = " + DecodeJwtPayload(accessToken));

      using HttpRequestMessage request = new(HttpMethod.Get, URI);
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      Console.WriteLine($"Mgmt token prefix={accessToken[..20]}... len={accessToken.Length}");

      var httpResponseMessage = await _client.SendAsync(request, ct);

      if (!httpResponseMessage.IsSuccessStatusCode)
      {
        throw new Exception(await httpResponseMessage.Content.ReadAsStringAsync(ct));
      }

      var response = await httpResponseMessage.Content.ReadFromJsonAsync<UserResponseDto>(ct);

      return response;
    }


    private async Task<TokenResponseDto?> GetAccessTokenFromM2M(CancellationToken ct)
    {
      var URI = "oauth/token";

      var body = new
      {
        client_id = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID"),
        client_secret = Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET"),
        audience = "https://dev-motion-muse.eu.auth0.com/api/v2/",
        grant_type = "client_credentials",
        scope = "read:users read:user_idp_tokens"
      };

      var httpResponseMessage = await _client.PostAsJsonAsync(URI, body, ct);


      if (!httpResponseMessage.IsSuccessStatusCode)
      {
        throw new Exception(await httpResponseMessage.Content.ReadAsStringAsync(ct));
      }

      var response = await httpResponseMessage.Content.ReadFromJsonAsync<TokenResponseDto>(ct);

      return response;
    }
  }
}
