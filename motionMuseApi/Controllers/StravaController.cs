using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using motionMuseApi.Services;

namespace motionMuseApi.Controllers
{
  [ApiController]
  [Route("api/strava")]
  [Authorize]
  public class StravaController(IStravaAuthService service) : ControllerBase
  {
    [HttpGet]
    [Route("_debug/m2m")]
    public async Task<string?> Ping()
    {
      var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

      if (string.IsNullOrEmpty(auth0UserId))
      {
        return "";
      }

      var stravaAccessToken = await service.GetStravaValidAccessTokenAsync(auth0UserId);

      if (stravaAccessToken is null)
      {
        // Query the Oauth via M2M to get the correct token
        var m2mApiToken = await service.GetManagementApiToken() ?? throw new Exception("No token response from M2M");

        var userFromOAuth = await service.GetUserFromOAuth(auth0UserId, m2mApiToken) ?? throw new Exception("No user found on Oauth");


        // TODO récupérer le bon stravaAccess Token depuis le UserResponseModeDTO car la on ne parse pas comme i faut c'est pas dans provider strava
        stravaAccessToken = userFromOAuth.Identities.FirstOrDefault(u => u.Provider == "strava")?.AccessToken;
      }

      return stravaAccessToken;
    }
  }
}