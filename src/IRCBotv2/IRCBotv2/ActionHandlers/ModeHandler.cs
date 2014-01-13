using System.Threading;
using IRCBotv2.Core;
using IRCBotv2.Enums;
using IRCBotv2.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IRCBotv2.ActionHandlers
{
	public class ModeHandler
	{
		public static List<Task> GetTasks(StreamWriter writer, ChatMessage message, List<Channel> channels)
		{
			var tasks = new List<Task>();

			if (message.MessageSenderType == MessageSenderType.Myself
				&& message.Recipient.Nick.Equals(message.Sender.Nick, StringComparison.OrdinalIgnoreCase))
			{
				tasks.Add(new Task(() => RespondToLoginComplete(writer, channels)));
			}

			return tasks;
		}

		private static void RespondToLoginComplete(StreamWriter writer, IEnumerable<Channel> channels)
		{
			foreach (var chan in channels)
			{
				if (chan.IsPasswordProtected)
				{
					writer.WriteOutput(string.Format("JOIN {0} {1}", chan.Name, chan.Password));
				}
				else
				{
					writer.WriteOutput(string.Format("JOIN {0}", chan.Name));
				}
				
				Thread.Sleep(1000);
			}
		}
	}
}
