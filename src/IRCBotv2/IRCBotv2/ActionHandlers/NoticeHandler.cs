using IRCBotv2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IRCBotv2.Extensions;

namespace IRCBotv2.ActionHandlers
{
	public static class NoticeHandler
	{
		public static List<Task> GetTasks(StreamWriter writer, ChatMessage message, string nick, string user)
		{
			var tasks = new List<Task>();

			if (message.Recipient.RawData.Equals("auth", StringComparison.OrdinalIgnoreCase)
				&& message.Message.Contains("Checking ident..."))
			{
				tasks.Add(new Task(() => RespondToIdent(writer, nick, user)));
			}

			return tasks;
		}

		private static void RespondToIdent(StreamWriter writer, string nick, string user)
		{
			writer.WriteOutput("NICK " + nick);
			writer.WriteOutput(string.Format("USER {0} {0} bla :{0}", user));
		}
	}
}
