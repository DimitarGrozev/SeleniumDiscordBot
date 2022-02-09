using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Wix
{
	public class AuthorizationResponse
	{
		[JsonPropertyName("subscription")]
		public Subscription Subscription { get; set; }
	}
}
