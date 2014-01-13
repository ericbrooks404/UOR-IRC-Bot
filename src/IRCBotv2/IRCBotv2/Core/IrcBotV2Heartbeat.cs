using System;
using System.Collections.Generic;
using System.Linq;
using IRCBotv2.Core;
using IRCBotv2.Extensions;

namespace IrcBotV2.Core
{
	public partial class IrcBotV2
	{
		private TimeSpan SigilUpdateFrequency { get; set; }

		private DateTime NextSigilUpdateTime { get; set; }

		private TimeSpan HeartbeatFrequency { get; set; }

		private DateTime NextHeartbeatTime { get; set; }

		private TimeSpan SaveFrequency { get; set; }

		private DateTime NextSaveTime { get; set; }

		private TimeSpan StatusCheckFrequency { get; set; }

		private DateTime NextStatusCheckTime { get; set; }

		private TimeSpan StatusRefreshFrequency { get; set; }

		private DateTime NextStatusRefreshTime { get; set; }

		private TimeSpan BodReminderCheckFrequency { get; set; }

		private DateTime NextBodReminderCheckTime { get; set; }

		public delegate void HeartbeatEventHandler(object sender, EventArgs args);

		public event HeartbeatEventHandler OnHeartbeat;

		private void InitializeHeartbeat()
		{
			this.HeartbeatFrequency = TimeSpan.FromSeconds(1);
			this.NextHeartbeatTime = DateTime.UtcNow.Add(this.HeartbeatFrequency);

			this.SigilUpdateFrequency = TimeSpan.FromMinutes(1);
			this.NextSigilUpdateTime = DateTime.UtcNow.Add(this.SigilUpdateFrequency);

			this.SaveFrequency = TimeSpan.FromMinutes(30);
			this.NextSaveTime = DateTime.UtcNow.Add(this.SaveFrequency);

			this.StatusRefreshFrequency = TimeSpan.FromHours(24);
			this.NextStatusRefreshTime = DateTime.UtcNow.Add(this.StatusRefreshFrequency);

			this.StatusCheckFrequency = TimeSpan.FromMinutes(1);
			this.NextStatusCheckTime = DateTime.UtcNow.Add(this.StatusCheckFrequency);

			this.BodReminderCheckFrequency = TimeSpan.FromMinutes(1);
			this.NextBodReminderCheckTime = DateTime.UtcNow.Add(this.BodReminderCheckFrequency);
		}

		private void InitializeHeartBeatEvents()
		{
			this.OnHeartbeat += this.UpdateSigils;
			this.OnHeartbeat += this.CheckSigilExpiration;
			this.OnHeartbeat += this.CheckSave;
			this.OnHeartbeat += this.CheckStatus;
			this.OnHeartbeat += this.CheckBodReminders;
		}

		private void StopHeartBeat()
		{
			this.NextHeartbeatTime = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5));
		}

		private void CheckBodReminders(object o, EventArgs args)
		{
			var remindersSent = new List<FullName>();

			foreach (var reminder in this.Settings.BodReminderList)
			{
				var player = reminder.Key;
				var reminderTime = reminder.Value;

				if (DateTime.Compare(DateTime.UtcNow, reminderTime) > 0)
				{
					this.Writer.WriteOutput(string.Format("PRIVMSG {0} :Time for another BOD!", player.Nick));

					remindersSent.Add(player);
				}
			}

			remindersSent.ForEach(x => this.Settings.BodReminderList.Remove(x));

			this.NextBodReminderCheckTime = DateTime.UtcNow.Add(this.BodReminderCheckFrequency);
		}

		private void CheckStatus(object o, EventArgs args)
		{
			if (DateTime.Compare(DateTime.UtcNow, this.NextStatusCheckTime) <= 0)
			{
				return;
			}

			this.NextStatusCheckTime = this.NextStatusCheckTime.Add(this.StatusCheckFrequency);

			if (DateTime.Compare(DateTime.UtcNow, this.NextStatusRefreshTime) > 0)
			{
				this.NextStatusRefreshTime = this.NextStatusRefreshTime.Add(this.StatusRefreshFrequency);

				this.StatusList = this.StatusService.GetNewUpdates().ToList();
			}

			foreach (var status in this.StatusList)
			{
				var isAfterStart = DateTime.Compare(DateTime.UtcNow, status.StartTime) > 0;
				var isBeforeFinish = DateTime.Compare(DateTime.UtcNow, status.EndTime) < 0;
				var isTimeForChat = DateTime.Compare(DateTime.UtcNow, status.NextChatTime) > 0;

				if (isAfterStart && isBeforeFinish && isTimeForChat)
				{
					status.NextChatTime = DateTime.UtcNow.Add(status.ChatFrequency);

					var status1 = status;
					var appropriateChannels = this.Settings.Channels.Where(x => x.SecurityGroup == status1.SecurityGroup);

					this.Writer.SendMessageToChannels(appropriateChannels, this.ActionQueue, status.Message);
				}
				else if (!isBeforeFinish)
				{
					this.StatusService.DeleteOldStatuses(status.Id);
				}
			}
		}

		private void UpdateSigils(object o, EventArgs args)
		{
			if (DateTime.Compare(DateTime.UtcNow, this.NextSigilUpdateTime) > 0)
			{
				this.NextSigilUpdateTime = DateTime.UtcNow.Add(this.SigilUpdateFrequency);

				this.SigilsService.UpdateSigilInfo(this.Writer, this.ActionQueue, this.Settings);
			}
		}

		private void CheckSave(object o, EventArgs args)
		{
			if (DateTime.Compare(DateTime.UtcNow, this.NextSaveTime) > 0)
			{
				this.NextSaveTime = DateTime.UtcNow.Add(this.SaveFrequency);
				this.SaveSettings();
			}
		}

		private void CheckSigilExpiration(object o, EventArgs args)
		{
			foreach (var sigil in this.Settings.SigilStatus)
			{
				sigil.CheckSigilLandmarks(this.Settings.Channels, this.Writer, this.ActionQueue);
			}
		}
	}
}
