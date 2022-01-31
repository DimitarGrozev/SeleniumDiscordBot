using System.Text.Json.Serialization;

namespace Discordian.BotLauncher.Models
{
    public class Server
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }
    }
}
