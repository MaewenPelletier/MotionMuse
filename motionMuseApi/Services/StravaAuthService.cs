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

      return entity?.AccessToken ?? null;
    }

    public async Task<string?> GetManagementApiToken(CancellationToken ct = default)
    {
      // Query the Oauth via M2M to get the correct token
      var tokenResponse = await GetAccessTokenFromM2M(ct);

      return tokenResponse?.AccessToken ?? null;
    }

    public async Task<UserResponseDto?> GetUserFromOAuth(string auth0UserId, string token, CancellationToken ct = default)
    {
      var encodedAuth0UserId = Uri.EscapeDataString(auth0UserId);
      var URI = $"/api/v2/users/{encodedAuth0UserId}";

      using HttpRequestMessage request = new(HttpMethod.Get, URI);
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
        grant_type = "client_credentials"
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
