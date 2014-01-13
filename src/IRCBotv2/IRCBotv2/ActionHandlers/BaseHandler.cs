using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCBotv2.Core;
using IRCBotv2.Enums;

namespace IRCBotv2.ActionHandlers
{
	public class BaseHandler
	{
		public static SecurityGroup GetSecurityGroup(ChatMessage message, List<Channel> channels, List<string> friendHostnames)
		{
			var securityGroup = SecurityGroup.None;
			var sourceIsChannel = (message.Sender.Nick.Contains("#"));

			// if sender is a private message, we can't authenticate their security level
			if (!sourceIsChannel)
			{
				var userIsFriend = friendHostnames.Any(x => x.Equals(message.Sender.Hostname, StringComparison.OrdinalIgnoreCase));

				securityGroup = userIsFriend ? SecurityGroup.Staff : SecurityGroup.Public;
			}
			else
			{
				// otherwise, status was requested in a chat
				var requestingChannel = channels.SingleOrDefault(x => message.Sender.Nick.Equals(x.Name, StringComparison.OrdinalIgnoreCase));

				if (requestingChannel == null)
				{
					Console.WriteLine("- An error occured finding source of !news request.");

					return SecurityGroup.None;
				}

				securityGroup = requestingChannel.SecurityGroup;
			}

			return securityGroup;
		}
	}
}
