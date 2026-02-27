using motionMuseApi.Models;

namespace motionMuseApi.Services
{
  public interface IStravaAuthService
  {
    Task<string?> GetStravaValidAccessTokenAsync(string auth0UserId, CancellationToken ct = default);
    Task<UserResponseDto?> GetUserFromOAuth(string auth0UserId, string token, CancellationToken ct = default);
  }
}