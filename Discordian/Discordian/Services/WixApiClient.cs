using Discordian.Core.Helpers;
using Discordian.Core.Models.Wix;
using Discordian.Utilities;
using System;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Discordian.Services
{
    public class WixApiClient
    {
        private readonly AppConfig appConfig = new AppConfig();
        private readonly string AuthenticateURL = "https://discordian.app/_functions/authenticate";
        private readonly string SubscriptionURL = "https://discordian.app/_functions/subscription?email={0}";

        public async Task<AuthenticationResponse> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException();
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");

            var content = new HttpStringContent(await Json.StringifyAsync(new User { email = email, password = password }), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.RequestUri = new Uri(AuthenticateURL);
            httpRequestMessage.Headers.Add("discordian-api-key", this.appConfig.ApiKey.DiscordianApiKey);
            httpRequestMessage.Content = content;
            httpRequestMessage.Method = HttpMethod.Post;

            var response = await httpClient.SendRequestAsync(httpRequestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();
            var authenticationResponse = await Json.ToObjectAsync<AuthenticationResponse>(responseContent);

            return authenticationResponse;
        }

        public async Task<Subscription> GetSubscriptionAsync(string email)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");

            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.RequestUri = new Uri(string.Format(SubscriptionURL, email));
            httpRequestMessage.Headers.Add("discordian-api-key", this.appConfig.ApiKey.DiscordianApiKey);
            httpRequestMessage.Method = HttpMethod.Get;

            var response = await httpClient.SendRequestAsync(httpRequestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();
            var subscriptionResponse = await Json.ToObjectAsync<SubscriptionResponse>(responseContent);

            return subscriptionResponse?.Subscription;
        }
    }
}
