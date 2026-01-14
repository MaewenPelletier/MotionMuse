using System.Text.Json.Serialization;

namespace motionMuseApi.Models
{
  public class UserResponseDto
  {
    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }

    [JsonPropertyName("identities")]
    public required Identity[] Identities { get; set; }

  }

  public class Identity
  {
    public required string Provider {get; set;}

    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }

    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; }
  }
}