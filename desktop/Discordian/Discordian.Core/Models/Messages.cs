using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models
{
    public class Messages
    {
        [JsonPropertyName("sentences")]
        public List<string> Sentences { get; set; }
    }
}
