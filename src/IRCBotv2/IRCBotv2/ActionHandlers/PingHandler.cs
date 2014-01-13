using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCBotv2.Core;
using IRCBotv2.Extensions;

namespace IRCBotv2.ActionHandlers
{
	public static class PingHandler
	{
		public static List<Task> GetTasks(StreamWriter writer, ChatMessage message)
		{
			var tasks = new List<Task>();

			tasks.Add(new Task(() => RespondToPing(writer, message)));

			return tasks;
		}

		private static void RespondToPing(StreamWriter writer, ChatMessage message)
		{
			writer.WriteOutput(string.Format("PONG :{0}", message.Message));
		}
	}
}
