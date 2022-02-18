using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Wix
{
	public class AuthenticationResponse
	{
		[JsonPropertyName("error")]
		public string Error{ get; set; }
	}
}
