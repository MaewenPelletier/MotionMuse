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
    [Route("GetUser")]
    public async Task<string?> GetUser()
    {
      var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

      if (string.IsNullOrEmpty(auth0UserId))
      {
        return "";
      }

      var stravaAccessToken = await service.GetStravaValidAccessTokenAsync(auth0UserId);

     

      // Je call le endpoint de strava pour récupérer les info dont j'aurais besoin avec un token OK

      // je renvoie un DTO updaté au front
      return stravaAccessToken;
    }
  }
}
