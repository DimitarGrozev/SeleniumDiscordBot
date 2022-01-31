using Discordian.Core.Helpers;
using Discordian.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Discordian.Services
{
    public static class DiscordianBotConsoleClient
    {
        public static async Task<string> GetTokenForNewAccountAsync(string email, string password)
        {
            var token = string.Empty;

            var valueSet = new ValueSet();
            valueSet.Add("request", "login");
            valueSet.Add("email", email);
            valueSet.Add("password", password);

            if (App.Connection != null)
            {
                var response = await App.Connection.SendMessageAsync(valueSet);
                token = response.Message["token"].ToString().Replace("\"", "");

                if (!string.IsNullOrEmpty(token))
                {
                    await DiscordianDbContext.SaveAccountAsync(email, password, token);
                }
            }

            return token;
        }

        public static async Task StartBotAsync(Bot bot, Messages messages)
        {
            var valueSet = new ValueSet();
            valueSet.Add("request", "start");
            valueSet.Add("messages", await Json.StringifyAsync(messages));
            valueSet.Add("bot", await Json.StringifyAsync(bot));

            if (App.Connection != null)
            {
                var response = await App.Connection.SendMessageAsync(valueSet);
            }
        }

        public static async Task StopBotAsync(Guid id)
        {
            var valueSet = new ValueSet();
            valueSet.Add("request", "stop");
            valueSet.Add("id", id.ToString());

            if (App.Connection != null)
            {
                var response = await App.Connection.SendMessageAsync(valueSet);
            }
        }
    }
}
