using System.Text.Json.Serialization;

namespace ConsoleApp4
{
    public class Configuration
    {
        [JsonPropertyName("messageDelay")]
        public int MessageDelay { get; set; }

        [JsonPropertyName("serverSwitchDelay")]
        public int ServerSwitchDelay{ get; set; }

        [JsonPropertyName("messagesFilePath")]
        public string MessagesFilePath { get; set; }
    }
}
