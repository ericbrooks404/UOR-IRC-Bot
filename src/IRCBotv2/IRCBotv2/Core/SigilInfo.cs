using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IRCBotv2.Enums;
using IRCBotv2.Extensions;

namespace IRCBotv2.Core
{
	public class SigilInfo
	{
		private const string TimeSpanFormatter = @"dd\.hh\:mm";

		public string CityName { get; set; }

		public Faction Corrupted { get; set; }

		public Faction Corrupting { get; set; }

		public Faction Faction { get; set; }

		public DateTime CorruptionStartTime { get; set; }

		public bool WarnedCorruptionHalfHourReset { get; set; }

		public bool WarnedCorruptionTenMinuteReset { get; set; }

		public DateTime LastCaptureTime { get; set; }

		public bool WarnedFiveHoursReset { get; set; }

		public bool WarnedHourReset { get; set; }

		public bool WarnedHalfHourReset { get; set; }

		public bool WarnedReset { get; set; }

		public bool IsCorrupting
		{
			get { return TimeUntilCorrupted != TimeSpan.Zero; }
		}

		public bool IsCaptured
		{
			get { return TimeUntilAvailable != TimeSpan.Zero; }
		}

		public TimeSpan TimeUntilAvailable 
		{ 
			get
			{
				if (this.LastCaptureTime == DateTime.MinValue)
				{
					return TimeSpan.Zero;
				}

				var availableTime = this.LastCaptureTime.AddDays(3);
				var timeUntilAvailable = availableTime.Subtract(DateTime.UtcNow);

				return timeUntilAvailable;
			}
		}

		public TimeSpan TimeUntilCorrupted
		{
			get
			{
				if (this.CorruptionStartTime == DateTime.MinValue)
				{
					return TimeSpan.Zero;
				}

				var corruptionFinishTime = this.CorruptionStartTime.AddHours(7);
				var timeUntilCorrupted = corruptionFinishTime.Subtract(DateTime.UtcNow);

				return timeUntilCorrupted;
			}
		}

	    public void ResetCorruptionWarnings()
		{
			this.WarnedCorruptionHalfHourReset = false;
			this.WarnedCorruptionTenMinuteReset = false;
		}

		public void ResetCaptureWarnings()
		{
			this.WarnedCorruptionHalfHourReset = false;
			this.WarnedCorruptionTenMinuteReset = false;
			this.WarnedFiveHoursReset = false;
			this.WarnedHourReset = false;
			this.WarnedHalfHourReset = false;
			this.WarnedReset = false;
		}

		public string GetSigilStatusMessage()
		{
			var sigilStatusString = string.Format("{0} is owned by {1}", this.CityName, this.Faction);

			if (this.LastCaptureTime != DateTime.MinValue)
			{
				sigilStatusString += string.Format(" still for {0}", this.TimeUntilAvailable.ToString(TimeSpanFormatter));
			}

			if (this.Corrupting != Faction.None)
			{
				if (this.CorruptionStartTime == DateTime.MinValue)
				{
					sigilStatusString += string.Format(" and is corrupting for {0}", this.Corrupting);
				}
				else
				{
					sigilStatusString += string.Format(" and will be finished corrupting for {0} in {1}", this.Corrupting,
													   this.TimeUntilCorrupted.ToString(TimeSpanFormatter));
				}
			}

			if (this.Corrupted != Faction.None)
			{
				sigilStatusString += string.Format(" and is corrupted for {0}", this.Corrupted);
			}

			sigilStatusString += ".";

			return sigilStatusString;
		}

		public void CheckSigilLandmarks(List<Channel> channels, StreamWriter writer, Queue<Task> actionQueue)
		{
			var isLessThanFiveHours = (this.TimeUntilAvailable < TimeSpan.FromHours(5));
			var isLessThanHour = (this.TimeUntilAvailable < TimeSpan.FromMinutes(60));
			var isLessThanHalfHour = (this.TimeUntilAvailable < TimeSpan.FromMinutes(30));

			var isLessThanHalfHourCorruption = (this.TimeUntilCorrupted < TimeSpan.FromMinutes(30));
			var isLessThanTenMinutesCorruption = (this.TimeUntilCorrupted < TimeSpan.FromMinutes(10));

			if (isLessThanFiveHours && !this.WarnedFiveHoursReset)
			{
				this.WarnedFiveHoursReset = true;

				var message = string.Format("{0} will be available to steal in {1}", this.CityName, this.TimeUntilAvailable.ToString(TimeSpanFormatter));
				writer.SendMessageToChannels(channels, actionQueue, message);
			}

			if (isLessThanHour && !this.WarnedHourReset == false)
			{
				this.WarnedHourReset = true;
				var message = string.Format("{0} will be available to steal in {1}", this.CityName, this.TimeUntilAvailable.ToString(TimeSpanFormatter));
				writer.SendMessageToChannels(channels, actionQueue, message);
			}

			if (isLessThanHalfHour && !this.WarnedHalfHourReset)
			{
				this.WarnedHalfHourReset = true;
				var message = string.Format("{0} will be available to steal in {1}", this.CityName, this.TimeUntilAvailable.ToString(TimeSpanFormatter));
				writer.SendMessageToChannels(channels, actionQueue, message);
			}


			if (this.IsCorrupting && isLessThanHalfHourCorruption && !this.WarnedCorruptionHalfHourReset)
			{
				this.WarnedCorruptionHalfHourReset = true;
				var message = string.Format("{0} will be corrupted for {2} in {1}", this.CityName, this.TimeUntilCorrupted.ToString(TimeSpanFormatter), this.Corrupting);
				writer.SendMessageToChannels(channels, actionQueue, message);
			}

			if (this.IsCorrupting && isLessThanTenMinutesCorruption && !this.WarnedCorruptionTenMinuteReset)
			{
				this.WarnedCorruptionTenMinuteReset = true;
				var message = string.Format("{0} will be corrupted for {2} in {1}", this.CityName, this.TimeUntilCorrupted.ToString(TimeSpanFormatter), this.Corrupting);
				writer.SendMessageToChannels(channels, actionQueue, message);
			}
		}
	}
}
