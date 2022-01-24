using System;
using System.Collections.Generic;
using System.Text;

namespace Discordian.Core.Models.Discord
{
	public class Server
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string Icon { get; set; }

		public bool Owner { get; set; }

		public string Permissions { get; set; }

		public List<string> Features { get; set; }
	}
}
