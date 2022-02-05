using Discordian.Core.Helpers;
using Discordian.Core.Models;
using Discordian.Core.Models.Charts;
using Discordian.Core.Models.Discord;
using Discordian.Utilities;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discordian.Services
{
    public static class DiscordApiClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly Uri _baseUri = new Uri("https://discord.com/api/v8/", UriKind.Absolute);

        private static IAsyncPolicy<HttpResponseMessage> ResponsePolicy { get; } =
          Policy
              .Handle<IOException>()
              .Or<HttpRequestException>()
              .OrResult<HttpResponseMessage>(m => (int)m.StatusCode == 429)
              .OrResult(m => m.StatusCode == HttpStatusCode.RequestTimeout)
              .OrResult(m => m.StatusCode >= HttpStatusCode.InternalServerError)
              .WaitAndRetryAsync(
                  8,
                  (i, result, context) =>
                  {
                      if ((int)result.Result?.StatusCode == 429)
                      {
                          if (i > 3)
                          {
                              var retryAfterDelay = result.Result.Headers.RetryAfter?.Delta;
                              if (retryAfterDelay != null)
                                  return retryAfterDelay.Value + TimeSpan.FromSeconds(1);
                          }
                      }

                      return TimeSpan.FromSeconds(Math.Pow(2, i) + 1);
                  },
                  (_, _, _, _) => Task.CompletedTask
        );

        public static async Task<List<Data>> GetBotMessageCountForLastSevenDaysAsync(DiscordData discordData, string token)
        {
            var after = Snowflake.FromDate(new DateTimeOffset(DateTime.Today.AddDays(-8))).ToString();

            var url = new UrlBuilder()
              .SetPath($"guilds/{discordData.ServerId}/messages/search?author_id={discordData.UserId}&channel_id={discordData.ChannelId}&min_id={after}")
              .Build();

            var response = await GetResponseAsync(url, token);
            var content = await response.Content.ReadAsStringAsync();
            var result = await Json.ToObjectAsync<ServerSearchResult>(content);

            var statistics = GroupStatistics(result);

            return statistics;
        }

        public static async Task<bool> ServerExistsAsync(string serverName, string token)
        {
            var serverExists = true;

            try
            {
                await GetServerIdAsync(serverName, token);
            }
            catch (ArgumentException ex)
            {
                serverExists = false;
            }

            return serverExists;
        }

        public static async Task<DiscordData> GetDiscordDataForBot(string serverName, string channelName, string token)
        {
            var serverId = await GetServerIdAsync(serverName, token);
            var channelId = await GetChannelIdByNameAsync(serverId, serverName, channelName, token);
            var userId = await GetUserIdAsync(token);

            var discordData = new DiscordData { ServerId = serverId, ChannelId = channelId, UserId = userId };

            return discordData;
        }

        public static async Task<int> GetBotMentionsCountInChannelAsync(DiscordData data, string token)
        {
            var url = new UrlBuilder()
               .SetPath($"guilds/{data.ServerId}/messages/search?channel_id={data.ChannelId}&mentions={data.UserId}")
               .Build();

            var response = await GetResponseAsync(url, token);
            var content = await response.Content.ReadAsStringAsync();
            var result = await Json.ToObjectAsync<ServerSearchResult>(content);

            return result.Total_Results;
        }

        public static async Task<int> GetBotMessageCountInChannelAsync(DiscordData data, string token)
        {
            var url = new UrlBuilder()
                .SetPath($"guilds/{data.ServerId}/messages/search?channel_id={data.ChannelId}&author_id={data.UserId}")
                .Build();

            var response = await GetResponseAsync(url, token);
            var content = await response.Content.ReadAsStringAsync();
            var result = await Json.ToObjectAsync<ServerSearchResult>(content);

            return result.Total_Results;
        }

        public static async Task<string> GetUserIdAsync(string token)
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

        public static async Task<string> GetChannelIdByNameAsync(string serverId, string serverName, string channelName, string token)
        {
            var channels = await GetChannelsInServerAsync(serverId, token);
            var channel = channels.FirstOrDefault(c => c.Name.Contains(channelName));

            if (channel != null)
            {
                return channel.Id;
            }

            throw new ArgumentException($"Channel \"{channelName}\" does not exist in server \"{serverName}\"!");
        }

        public static async Task<List<Channel>> GetChannelsInServerAsync(string serverId, string token)
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


        public static async Task<List<Core.Models.Discord.Server>> GetServersForUserAsync(string token)
        {
            var url = new UrlBuilder()
                .SetPath("users/@me/guilds")
                .SetQueryParameter("limit", "100")
                .Build();

            var response = await GetResponseAsync(url, token);
            var content = await response.Content.ReadAsStringAsync();
            var servers = await Json.ToObjectAsync<List<Core.Models.Discord.Server>>(content);

            return servers;
        }

        public static async Task<string> GetServerIdAsync(string serverName, string token)
        {
            var servers = await GetServersForUserAsync(token);

            var server = servers.FirstOrDefault(s => s.Name == serverName);

            if (server != null)
            {
                return server.Id;
            }

            throw new ArgumentException($"Server \"{serverName}\" does not exist!");
        }

        private static async ValueTask<HttpResponseMessage> GetResponseAsync(string url, string token, CancellationToken cancellationToken = default)
        {
            return await ResponsePolicy.ExecuteAsync(async innerCancellationToken =>
            {
                return await GetInternalResponseAsync(url, token);

            }, cancellationToken);
        }

        private static async ValueTask<HttpResponseMessage> GetInternalResponseAsync(string url, string token, CancellationToken cancellationToken = default)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUri, url)))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(token);

                var response = await httpClient.SendAsync(request);

                return response;
            }
        }

        private static List<Data> GroupStatistics(ServerSearchResult result)
        {
            var statistics = new List<Data>();

            var messagesPerDay = new Dictionary<DateTime, Data>
            {
                {DateTime.Today.AddDays(-6), new Data{Category = DateTime.Today.AddDays(-6).DayOfWeek.ToString()} },
                {DateTime.Today.AddDays(-5), new Data{Category = DateTime.Today.AddDays(-5).DayOfWeek.ToString()} },
                {DateTime.Today.AddDays(-4), new Data{Category = DateTime.Today.AddDays(-4).DayOfWeek.ToString()} },
                {DateTime.Today.AddDays(-3), new Data{Category = DateTime.Today.AddDays(-3).DayOfWeek.ToString()} },
                {DateTime.Today.AddDays(-2), new Data{Category = DateTime.Today.AddDays(-2).DayOfWeek.ToString()} },
                {DateTime.Today.AddDays(-1), new Data{Category = DateTime.Today.AddDays(-1).DayOfWeek.ToString()} },
                {DateTime.Today, new Data{ Category = DateTime.Today.DayOfWeek.ToString()} },
            };

            foreach (var messageWrapper in result.Messages)
            {
                foreach (var message in messageWrapper)
                {
                    if (messagesPerDay.ContainsKey(message.Timestamp.Date))
                    {
                        messagesPerDay[message.Timestamp.Date].Value++;
                    }
                }
            }

            statistics.AddRange(messagesPerDay.Values);

            statistics.ForEach(s => s.LabelProperty = s.Value == 0 ? "" : s.Value.ToString());

            return statistics;
        }
    }
}
