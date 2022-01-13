using ConsoleApp4.Models;
using System.Text.Json.Serialization;

namespace ConsoleApp4
{
    public class Server
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }
    }
}
