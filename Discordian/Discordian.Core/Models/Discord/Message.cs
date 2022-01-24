using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Discord
{
	public class Message
	{
        public string Id { get; set; }

        public int Type { get; set; }

        public string Content { get; set; }

        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; }

        public Author Author { get; set; }

        public List<object> Mentions { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
