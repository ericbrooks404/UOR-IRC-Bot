using System;
using System.Linq;
using IRCBotv2.Core;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IRCBotv2.Enums;

namespace IRCBotv2.ActionHandlers
{
	public static class ChanListHandler
	{
		public static List<Task> GetTasks(StreamWriter writer, ChatMessage message, List<Channel> channels, string nick)
		{
			var tasks = new List<Task>();

			tasks.Add(new Task(() => RespondToJoinedChan(message, channels, nick)));

			return tasks;
		}

		private static void RespondToJoinedChan(ChatMessage message, List<Channel> channels, string nick)
		{
			var delimiter = message.Recipient.RawData.Contains("@")
				                ? " @ "
				                : " = ";

			var chan = message.Recipient.RawData.Replace(nick + delimiter, string.Empty);

			if (string.IsNullOrWhiteSpace(chan) || channels.Any(x => x.Name.Equals(chan, StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}

			channels.Add(new Channel()
				{
					IsPasswordProtected = false,
					Name = chan,
					Password = string.Empty,
					SecurityGroup = SecurityGroup.Public
				});
		}
	}
}
