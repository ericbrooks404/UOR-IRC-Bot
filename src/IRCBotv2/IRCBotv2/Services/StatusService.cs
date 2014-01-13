using System;
using System.Collections.Generic;
using IRCBotv2.Core;
using IRCBotv2.Enums;

namespace IRCBotv2.Services
{
	public class StatusService
	{
		// stubbed until Chris can get me MySQL credentials
		public void DeleteOldStatuses(int id)
		{
			Console.WriteLine("- Purging expired status from MySQL...");
		}

		// stubbed until Chris can get me MySQL credentials
		public IEnumerable<StatusUpdate> GetNewUpdates()
		{
			Console.WriteLine("- Pulling new statuses from MySQL...");

			return new List<StatusUpdate>()
				{
					//new StatusUpdate()
					//	{
					//		Id = 0,
					//		StartTime = DateTime.UtcNow,
					//		EndTime = DateTime.UtcNow.AddHours(24),
					//		ChatFrequency = TimeSpan.FromMinutes(60),
					//		StatusType = StatusType.News,
					//		SecurityGroup = SecurityGroup.Public,
					//		Message = "Tonight's Friday Fight Night at Grimoire is a 1v1 standard tournament. Come get some! http://www.uorenaissance.com/phpBB/viewtopic.php?f=8&t=2708"
					//	}
				};
		}
	}
}
