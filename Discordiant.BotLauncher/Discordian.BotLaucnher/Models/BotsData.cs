using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discordian.BotLauncher.Models
{
    public class BotsData
    {
        [JsonPropertyName("bots")]
        public List<Bot> Bots { get; set; }
    }
}
