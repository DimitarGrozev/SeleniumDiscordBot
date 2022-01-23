using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discordian.BotLauncher.Models
{
    public class Bot
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("server")]
        public Server Server { get; set; }

        [JsonPropertyName("messageDelay")]
        public int MessageDelay { get; set; }

        [JsonPropertyName("credentials")]
        public Credentials Credentials { get; set; }
    }
}
