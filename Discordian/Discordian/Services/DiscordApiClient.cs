using Discordian.Core.Helpers;
using Discordian.Core.Models.Discord;
using Discordian.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discordian.Services
{
    public class DiscordApiClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private readonly Uri _baseUri = new Uri("https://discord.com/api/v8/", UriKind.Absolute);

        public async Task<int> GetBotMessageCountInChannelAsync(string serverName, string channelName, string token)
        {
            var serverId = await this.GetServerIdAsync(serverName, token);
            var channelId = await this.GetChannelIdByNameAsync(serverId, channelName, token);
            var userId = await this.GetUserIdAsync(token);

            var url = new UrlBuilder()
            .SetPath($"guilds/{serverId}/messages/search?channel_id={channelId}&author_id={userId}")
            .Build();

            var response = await GetResponseAsync(url, token);
            var content = await response.Content.ReadAsStringAsync();
            var result = await Json.ToObjectAsync<ServerSearchResult>(content);

            return result.Total_Results;
        }

        public async Task<string> GetUserIdAsync(string token)
        {
            var url = new UrlBuilder()
              .SetPath("users/@me")
              .Build();

            var response = await GetResponseAsync(url, token);
            var content = await response.Content.ReadAsStringAsync();
            var user = await Json.ToObjectAsync<User>(content);
            var userId = user.Id;

            return userId;
        }

        public async Task<string> GetChannelIdByNameAsync(string serverId, string channelName, string token)
        {
            var channels = await this.GetChannelsInServerAsync(serverId, token);
            var channel = channels.FirstOrDefault(c => c.Name.Contains(channelName));

            if (channel != null)
            {
                return channel.Id;
            }

            throw new ArgumentException("No such channel in server!");
        }

        public async Task<List<Channel>> GetChannelsInServerAsync(string serverId, string token)
        {
            var url = new UrlBuilder()
               .SetPath($"guilds/{serverId}/channels")
               .SetQueryParameter("limit", "100")
               .Build();

            var response = await GetResponseAsync(url, token);
            var content = await response.Content.ReadAsStringAsync();
            var channels = await Json.ToObjectAsync<List<Channel>>(content);

            return channels;
        }


        public async Task<List<Server>> GetServersForUserAsync(string token)
        {
            var url = new UrlBuilder()
                .SetPath("users/@me/guilds")
                .SetQueryParameter("limit", "100")
                .Build();

            var response = await GetResponseAsync(url, token);
            var content = await response.Content.ReadAsStringAsync();
            var guilds = await Json.ToObjectAsync<List<Server>>(content);

            return guilds;
        }

        public async Task<string> GetServerIdAsync(string serverName, string token)
        {
            var servers = await this.GetServersForUserAsync(token);

            var server = servers.FirstOrDefault(s => s.Name == serverName);

            if (server != null)
            {
                return server.Id;
            }

            throw new ArgumentException("User is not part of that server!");
        }

        private async ValueTask<HttpResponseMessage> GetResponseAsync(string url, string token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUri, url)))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(token);
                await Task.Delay(100);

                return await httpClient.SendAsync(request);

            }
        }
    }
}
