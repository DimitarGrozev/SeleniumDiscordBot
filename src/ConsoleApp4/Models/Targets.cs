using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConsoleApp4.Models
{
    public class Targets
    {
        [JsonPropertyName("servers")]
        public List<Server> Servers { get; set; }
    }
}
