using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Wix
{
	public class AuthenticationResponse
	{
		[JsonPropertyName("subscription")]
		public Subscription Subscription { get; set; }
	}
}
