using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConsoleApp4.Models
{
    public class Messages
    {
        [JsonPropertyName("sentences")]
        public List<string> Sentences { get; set; }
    }
}
