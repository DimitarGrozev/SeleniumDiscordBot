using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Wix
{
    public class Subscription
    {
        [JsonPropertyName("_id")]
        public Guid _id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("isPromotionSubscription")]
        public bool IsPromotionSubscription { get; set; }

        [JsonPropertyName("status")]
        public Status Status { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("plan")]
        public string Plan { get; set; }

        [JsonPropertyName("planPrice")]
        public string PlanPrice { get; set; }

        [JsonPropertyName("planId")]
        public Guid PlanId { get; set; }

        [JsonPropertyName("validUntil")]
        public string ValidUntil { get; set; }

        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("subscriptionId")]
        public Guid SubscriptionId { get; set; }

        [JsonPropertyName("planDescription")]
        public string PlanDescription { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("cancellationDate")]
        public DateTime CancellationDate { get; set; }

        [JsonPropertyName("userSubscriptions")]
        public List<string> UserSubscriptions { get; set; }
    }
}
