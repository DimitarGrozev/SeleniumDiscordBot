using System.Text.Json.Serialization;

namespace Discordian.BotLauncher.Models
{
    public class AppSettings
    {
        [JsonPropertyName("messagesFilePath")]
        public string MessagesFilePath { get; set; }
    }
}
