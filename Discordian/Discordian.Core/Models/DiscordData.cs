using System.Text.Json.Serialization;

namespace Discordian.Core.Models
{
    public class DiscordData
    {
        [JsonPropertyName("serverId")]
        public string ServerId { get; set; }

        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }
}