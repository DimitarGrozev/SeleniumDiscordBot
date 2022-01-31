using System.Text.Json.Serialization;

namespace Discordian.BotLauncher.Models
{
    public class Credentials
    {
        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [JsonPropertyName("Token")]
        public string Token { get; set; }
    }
}
