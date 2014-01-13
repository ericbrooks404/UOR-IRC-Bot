using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IRCBotv2.Core;

namespace IRCBotv2.Extensions
{
	public static class StreamWriterExtensions
	{
		public static void WriteOutput(this StreamWriter w, string s)
		{
			w.WriteLine(s);
			Console.WriteLine("> " + s);
		}

		public static void SendMessageToChannels(this StreamWriter writer, IEnumerable<Channel> channels, Queue<Task> actionQueue, string s)
		{
			foreach (var chan in channels)
			{
				var chan1 = chan;
				var task = new Task(() => writer.WriteOutput(string.Format("PRIVMSG {0} :{1}", chan1.Name, s)));

				actionQueue.Enqueue(task);
			}
		}
	}
}
