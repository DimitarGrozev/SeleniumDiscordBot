using System;
using System.Collections.Generic;
using System.Text;

namespace Discordian.BotLauncher.Utilities
{
	public static class Scripts
	{
		public static string getDiscordTokenFromBrowser = "let token;const getToken = () => {var iframe = document.createElement('iframe');iframe.onload = function () {var ifrLocalStorage = iframe.contentWindow.localStorage;token = ifrLocalStorage.getItem('token');};iframe.src = 'about:blank';document.body.appendChild(iframe);};getToken();while (!token) {}return token;";
	}
}
