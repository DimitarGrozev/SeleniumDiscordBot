using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Discordian.Core.Models.Wix
{
	public class User
	{
		public string email { get; set; }

		public string password { get; set; }
	}
}
