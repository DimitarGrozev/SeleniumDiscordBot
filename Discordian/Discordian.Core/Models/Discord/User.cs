using System;
using System.Collections.Generic;
using System.Text;

namespace Discordian.Core.Models.Discord
{
	public class User
	{
		public string Id { get; set; }

		public string Username { get; set; }

		public string Bio { get; set; }

		public bool Verified { get; set; }
	}
}
