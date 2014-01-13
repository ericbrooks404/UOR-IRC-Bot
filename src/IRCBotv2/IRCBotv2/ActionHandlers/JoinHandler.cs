using IRCBotv2.Core;
using IRCBotv2.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IRCBotv2.ActionHandlers
{
	public class JoinHandler
	{
		public static List<Task> GetTasks(StreamWriter writer, ChatMessage message, List<string> friendHostnames)
		{
			var tasks = new List<Task>();

			if (friendHostnames.Contains(message.Sender.Hostname))
			{
				tasks.Add(new Task(() => RespondToFriendJoin(writer, message)));
			}

			return tasks;
		}

		private static void RespondToFriendJoin(StreamWriter writer, ChatMessage message)
		{
			writer.WriteOutput(string.Format("MODE {0} +o {1}", message.Message, message.Sender.Nick));
		}
	}
}
