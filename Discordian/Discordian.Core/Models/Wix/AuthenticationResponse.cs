using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Wix
{
	public class AuthenticationResponse
	{
		[JsonPropertyName("isSubscribed")]
		public bool IsSubscribed { get; set; }
	}
}
