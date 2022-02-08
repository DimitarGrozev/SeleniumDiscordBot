using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Wix
{
	public class AuthenticationResponse
	{
		[JsonPropertyName("sessionToken")]
		public string SessionsToken { get; set; }
	}
}
