using IRCBotv2.Enums;

namespace IRCBotv2.Core
{
	public class Channel
	{
		public string Name { get; set; }

		public bool IsPasswordProtected { get; set; }

		public string Password { get; set; }

		public SecurityGroup SecurityGroup { get; set; }
	}
}
