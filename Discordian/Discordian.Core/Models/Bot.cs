﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models
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

        [JsonPropertyName("messagesFileName")]
        public string MessagesFileName { get; set; }

        [JsonPropertyName("credentials")]
        public Account Credentials { get; set; }

        [JsonPropertyName("discordData")]
        public DiscordData DiscordData{ get; set; }
    }
}
