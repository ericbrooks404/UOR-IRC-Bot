using System;
using IRCBotv2.Enums;

namespace IRCBotv2.Core
{
	public class StatusUpdate
	{
		public int Id { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public TimeSpan ChatFrequency { get; set; }

		public StatusType StatusType { get; set; }

		public SecurityGroup SecurityGroup { get; set; }

		public string Message { get; set; }

		public DateTime NextChatTime { get; set; }

		public override string ToString()
		{
			return string.Format("{0} - {1:g} - {2:g} - {3} - {4}",
			                     this.Id,
			                     this.StartTime,
			                     this.EndTime,
			                     this.SecurityGroup,
			                     this.Message);
		}
	}
}