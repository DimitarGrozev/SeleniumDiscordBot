using System.Text.Json.Serialization;

namespace Discordian.Core.Models
{
    public class Account
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
