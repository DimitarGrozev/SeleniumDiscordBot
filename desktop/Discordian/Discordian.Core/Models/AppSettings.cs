using System.Text.Json.Serialization;

namespace Discordian.Core.Models
{
    public class AppSettings
    {
        [JsonPropertyName("messagesFilePath")]
        public string MessagesFilePath { get; set; }
    }
}
