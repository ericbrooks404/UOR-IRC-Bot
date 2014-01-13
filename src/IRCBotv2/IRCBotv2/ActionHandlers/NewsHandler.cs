using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IRCBotv2.Core;
using IRCBotv2.Enums;
using IRCBotv2.Extensions;

namespace IRCBotv2.ActionHandlers
{
	public static class NewsHandler
	{
		private static bool HasCommand(ChatMessage message, string command)
		{
			return (message.Message.StartsWith(command, StringComparison.OrdinalIgnoreCase));
		}

		public static List<Task> GetTasks(StreamWriter writer,
		                                  ChatMessage message,
		                                  Settings settings,
		                                  Task dieAction,
		                                  Task saveSettingsTask,
		                                  Task updateSigilInfoTask,
		                                  DateTime uptime,
		                                  List<StatusUpdate> statusList)
		{
			var tasks = new List<Task>();

			if (HasCommand(message, "ls"))
			{
				tasks.Add(new Task(() => RespondToListRequest(writer, message, settings.Channels, statusList, settings.FriendHostnames)));
			}
			else if (HasCommand(message, "add"))
			{
				tasks.Add(new Task(() => RespondToAddRequest(writer, message, settings.Channels, statusList, settings.FriendHostnames)));
			}
			else if (HasCommand(message, "remove"))
			{
				tasks.Add(new Task(() => RespondToRemoveRequest(writer, message, settings.Channels, statusList, settings.FriendHostnames)));
			}
			else 
			{
				tasks.Add(new Task(() => RespondToNewsRequest(writer, message, settings.Channels, statusList, settings.FriendHostnames)));
			}

			return tasks;
		}

		public static void RespondToAddRequest(StreamWriter writer,
												ChatMessage message,
												List<Channel> channels,
												List<StatusUpdate> statusList,
												List<string> friendHostnames)
		{
			
		}

		private static void RespondToRemoveRequest(StreamWriter writer,
												ChatMessage message,
												List<Channel> channels,
												List<StatusUpdate> statusList,
												List<string> friendHostnames)
		{
			var startingRegex = @"starting \[(^\])";
		}

		private static void RespondToListRequest(StreamWriter writer,
												ChatMessage message,
												List<Channel> channels,
												List<StatusUpdate> statusList,
												List<string> friendHostnames)
		{
			var securityGroup = BaseHandler.GetSecurityGroup(message, channels, friendHostnames);

			if (securityGroup != SecurityGroup.Staff)
			{
				return;
			}

			writer.WriteOutput(string.Format("PRIVMSG {0} :Id - Start Time - End Time - Security Group - Message", message.Sender.Nick));
			Thread.Sleep(1000);
			foreach (var status in statusList)
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :{1}", message.Sender.Nick, status));
				Thread.Sleep(1000);
			}
		}

		private static void RespondToNewsRequest(StreamWriter writer,
												ChatMessage message,
												List<Channel> channels,
												List<StatusUpdate> statusList,
												List<string> friendHostnames)
		{
			var securityGroup = BaseHandler.GetSecurityGroup(message, channels, friendHostnames);

			var newsItems = statusList.Where(x => x.SecurityGroup == securityGroup);

			foreach (var news in newsItems)
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :{1}", message.Sender.Nick, news.Message));
			}
		}
	}
}
