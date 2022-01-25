using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Discord
{
	public class ServerSearchResult
	{
		[JsonPropertyName("total_results")]
		public int Total_Results { get; set; }

		[JsonPropertyName("messages")]
		public List<List<Message>> Messages { get; set; }
	}
}
