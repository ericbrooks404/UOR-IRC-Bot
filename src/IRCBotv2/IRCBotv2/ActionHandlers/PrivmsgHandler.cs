using IRCBotv2.Core;
using IRCBotv2.Enums;
using IRCBotv2.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IRCBotv2.ActionHandlers
{
	public static class PrivmsgHandler
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

			// adjust message.Nick field to point to channel if this is !command
			if (message.Message.StartsWith("!") && message.MessageSenderType == MessageSenderType.Other)
			{
				// message is from another user to a channel
				if (settings.Channels.Any(x => x.Name.Equals(message.Recipient.RawData, StringComparison.OrdinalIgnoreCase)))
				{
					message.Sender.Nick = message.Recipient.RawData;
					message.Message = message.Message.Remove(0, 1);
				}
			}

			if (HasCommand(message, "news"))
			{
				if (message.Message.Length < 5)
				{
					tasks.Add(new Task(() => writer.WriteOutput("news [list|add|remove]")));
					return tasks;
				}

				message.Message = message.Message.Substring(5);

				tasks.AddRange(NewsHandler.GetTasks(writer,
		                                  message,
		                                  settings,
		                                  dieAction,
		                                  saveSettingsTask,
		                                  updateSigilInfoTask,
		                                  uptime,
		                                  statusList));
			}
			if (HasCommand(message, "gotbod"))
			{
				tasks.Add(new Task(() => ResponseToGotBod(writer, message, settings)));
			}
			if (HasCommand(message, "ident"))
			{
				tasks.Add(new Task(() => RespondToIdentify(writer, message, settings.AuthenticatedHostnames)));
			}
			if (HasCommand(message, "addfriend"))
			{
				tasks.Add(new Task(() => RespondToAddFriend(writer, message, settings.AuthenticatedHostnames, settings.FriendHostnames)));
			}
			if (HasCommand(message, "die"))
			{
				tasks.Add(new Task(() => RespondToDie(writer, message, settings.AuthenticatedHostnames, dieAction)));
			}
			if (HasCommand(message, "join"))
			{
				tasks.Add(new Task(() => RespondToJoin(writer, message, settings.AuthenticatedHostnames)));
			}
			if (HasCommand(message, "part"))
			{
				tasks.Add(new Task(() => RespondToPart(writer, message, settings.AuthenticatedHostnames, settings.Channels)));
			}
			if (HasCommand(message, "savesettings"))
			{
				tasks.Add(new Task(() => RespondToSave(writer, message, settings.AuthenticatedHostnames, saveSettingsTask)));
			}
			if (HasCommand(message, "updatesigils"))
			{
				tasks.Add(new Task(() => RespondToUpdateSigils(writer, message, settings.AuthenticatedHostnames, updateSigilInfoTask)));
			}
			if (HasCommand(message, "uptime"))
			{
				tasks.Add(new Task(() => RespondToUptimeRequest(writer, message, uptime)));
			}
			if (HasCommand(message, "sigilstatus"))
			{
				tasks.Add(new Task(() => RespondToSigilStatus(writer, message, settings.SigilStatus)));
			}

			return tasks;
		}

		private static void ResponseToGotBod(StreamWriter writer, ChatMessage message, Settings bodReminderList)
		{
			writer.WriteOutput(string.Format("PRIVMSG {0} :I'll remind you to get another BOD in 6 hours", message.Sender.Nick));

			var thisCatsNextBodTime = DateTime.UtcNow.Add(TimeSpan.FromHours(6));

			bodReminderList.BodReminderList.Add(message.Sender, thisCatsNextBodTime);
		}

		private static void RespondToSigilStatus(StreamWriter writer, ChatMessage message, List<SigilInfo> sigils)
		{
			var args = message.Message.Split(' ');

			if (args.Length < 2) return;

			var city = message.Message.Substring(11).Trim().ToLower();

			if (string.IsNullOrEmpty(city)) return;

			lock (sigils)
			{
				if (city == "all")
				{
					foreach (var sigil in sigils)
					{
						var sigilStatusString = sigil.GetSigilStatusMessage();

						writer.WriteOutput(string.Format("PRIVMSG {0} :{1}", message.Sender.Nick, sigilStatusString));

						Thread.Sleep(1000);
					}
				}
				else
				{
					var sigil = sigils.SingleOrDefault(x => x.CityName.ToLower().Trim() == city);
				
					if (sigil == null)
					{
						writer.WriteOutput(string.Format("PRIVMSG {0} :That is not a thing.", message.Sender.Nick));
					}
					else
					{
						writer.WriteOutput(string.Format("PRIVMSG {0} :{1}", message.Sender.Nick, sigil.GetSigilStatusMessage()));
					}
				}
			}
		}

		private static void RespondToUptimeRequest(StreamWriter writer, ChatMessage message, DateTime uptime)
		{
			var timeSinceStart = DateTime.UtcNow.Subtract(uptime);

			writer.WriteOutput(string.Format("PRIVMSG {0} :I have been running for {1}.", message.Sender.Nick, timeSinceStart));
		}

		private static void RespondToUpdateSigils(StreamWriter writer, ChatMessage message, List<string> authenticatedHostnames, Task updateSigilInfoTask)
		{
			if (authenticatedHostnames.Contains(message.Sender.Hostname))
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :Updating sigils information...", message.Sender.Nick));
				updateSigilInfoTask.Start();
			}
			else
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :You are not authenticated.", message.Sender.Nick));
			}
		}

		private static void RespondToSave(StreamWriter writer, ChatMessage message, List<string> authenticatedHostnames, Task saveSettingsTask)
		{
			if (authenticatedHostnames.Contains(message.Sender.Hostname))
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :Fine, I'll save my settings.. :/", message.Sender.Nick));

				saveSettingsTask.Start();
			}
			else
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :You are not authenticated.", message.Sender.Nick));
			}
		}

		private static void RespondToPart(StreamWriter writer, ChatMessage message, List<string> authenticatedHostnames, List<Channel> channels)
		{
			if (authenticatedHostnames.Contains(message.Sender.Hostname))
			{
				var chanName = message.Message.ToLower().Replace("part ", string.Empty);
				writer.WriteOutput(string.Format("PRIVMSG {0} :Parting {1}", message.Sender.Nick, chanName));
				writer.WriteOutput(string.Format("PART {0}", chanName));

				var chan = channels.SingleOrDefault(x => x.Name.Equals(chanName, StringComparison.OrdinalIgnoreCase));
				if (chan != null)
				{
					channels.Remove(chan);
				}
			}
			else
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :You are not authenticated.", message.Sender.Nick));
			}
		}

		private static void RespondToJoin(StreamWriter writer, ChatMessage message, List<string> authenticatedHostnames)
		{
			if (authenticatedHostnames.Contains(message.Sender.Hostname))
			{
				var chanName = message.Message.ToLower().Replace("join ", string.Empty);
				writer.WriteOutput(string.Format("PRIVMSG {0} :Joining {1}", message.Sender.Nick, chanName));
				writer.WriteOutput(string.Format("JOIN {0}", chanName));
			}
			else
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :You are not authenticated.", message.Sender.Nick));
			}
		}

		private static void RespondToDie(StreamWriter writer, ChatMessage message, List<string> authenticatedHostnames, Task dieAction)
		{
			if (authenticatedHostnames.Contains(message.Sender.Hostname))
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :FINE DICKBAG!", message.Sender.Nick));

				dieAction.Start();
			}
			else
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :You are not authenticated.", message.Sender.Nick));
			}
		}

		private static void RespondToIdentify(StreamWriter writer, ChatMessage message, List<string> authenticatedHostnames)
		{
			if (authenticatedHostnames.Contains(message.Sender.Hostname))
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :You are already authenticated, silly.", message.Sender.Nick));
				return;
			}

			var identRegex = new Regex(@"ident ([a-zA-Z0-9]{3,25})", RegexOptions.IgnoreCase);

			var identMatch = identRegex.Match(message.Message);
			var authenticated = (identMatch.Success && identMatch.Groups[1].StringValue() == "c0me0ver");

			var response = (authenticated)
				               ? "You have been authenticated."
				               : "Sorry, the password was incorrect.";

			if (authenticated)
			{
				authenticatedHostnames.Add(message.Sender.Hostname);
			}

			writer.WriteOutput(string.Format("PRIVMSG {0} :{1}", message.Sender.Nick, response));
		}

		private static void RespondToAddFriend(StreamWriter writer, ChatMessage message, List<string> authentiatedHostnames, List<string> friendHostnames)
		{
			if (!authentiatedHostnames.Contains(message.Sender.Hostname))
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :You are not authenticated.", message.Sender.Nick));
				return;
			}

			var addFriendRegex = new Regex(@"addfriend ([^!]*?)!([^@]*?)@([^\s]*)");
			var addFriendMatch = addFriendRegex.Match(message.Message);
			var friendFullName = addFriendMatch.Success
				                    ? addFriendMatch.Groups[0].StringValue().GetFullName()
									: null;

			var friendHostname = friendFullName != null && !string.IsNullOrWhiteSpace(friendFullName.Hostname)
				                     ? friendFullName.Hostname
				                     : string.Empty;

			if (addFriendMatch.Success && !string.IsNullOrWhiteSpace(friendHostname))
			{
				friendHostnames.Add(friendHostname);
				writer.WriteOutput(string.Format("PRIVMSG {2} :{0} ({1}) is now my friend!", friendFullName.Nick.Replace("addfriend ", string.Empty), friendHostname, message.Sender.Nick));
			}
			else
			{
				writer.WriteOutput(string.Format("PRIVMSG {0} :I failed to parse your friend's name.", message.Sender));
			}
		}
	}
}
