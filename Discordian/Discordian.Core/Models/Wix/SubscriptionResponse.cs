using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Wix
{
    public class SubscriptionResponse
    {
        [JsonPropertyName("subscription")]
        public Subscription Subscription { get; set; }
    }
}
