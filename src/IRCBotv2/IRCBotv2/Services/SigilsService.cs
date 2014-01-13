using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;
using IRCBotv2.Core;
using IRCBotv2.Enums;
using IRCBotv2.Extensions;
using IRCBotv2.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace IRCBotv2.Services
{
	public class SigilsService
	{
		private const string SigilTableRegexString = 
			@"<tr>\n<td>([^<]*?)</td>\n<td>([^<]*?)</td>\n<td>([^<]*?)</td>\n<td>([^<]*?)</td>";

		private Regex SigilTableRegex { get; set; }

		public SigilsService()
		{
			this.SigilTableRegex = new Regex(SigilsService.SigilTableRegexString);
		}

		public List<SigilInfo> GetAllSigilInfo()
		{
			var sigils = new List<SigilInfo>();

			string data;

			try
			{
				using (var client = new WebClient())
				{
					client.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
					data = client.DownloadString(@"http://www.uorenaissance.com/?page=m_factionstatus");
				}
			}
			catch (WebException we)
			{
				Console.WriteLine();
				Console.WriteLine("- Web exception encountered when requesting sigil info!");
				Console.WriteLine();

				return null;
			}

			var matches = this.SigilTableRegex.Matches(data);

			if (matches.Count == 0)
			{
				Console.WriteLine();
				Console.WriteLine("---- Did not recieve sigil information from server! ----");
				Console.WriteLine();

				return null;
				//throw new DataMisalignedException("Did not find sigil strings in web request to factions page!");
			}

			Console.WriteLine("- Sigils were updated successfully");

			foreach (Match match in matches)
			{
				var cityName = match.Groups[1].StringValue().Trim();
				var faction = match.Groups[2].StringValue().Trim().ToLower();
				var corrupted = match.Groups[3].StringValue().Trim().ToLower();
				var corrupting = match.Groups[4].StringValue().Trim().ToLower();

				var sigil = new SigilInfo()
					{
						Faction = UorDotComHelper.GetFaction(faction),
						CityName = cityName,
						Corrupted = UorDotComHelper.GetFaction(corrupted),
						Corrupting = UorDotComHelper.GetFaction(corrupting)
					};

				sigils.Add(sigil);
			}

			return sigils;
		}

		private void TattleOnSigilActivity(SigilInfo newInfo,
											SigilInfo savedCity,
											StreamWriter writer,
											Queue<Task> actionQueue,
											Settings settings)
		{
			if (newInfo.Corrupting != savedCity.Corrupting)
			{
				if (newInfo.Corrupting == Faction.None && newInfo.Corrupted == savedCity.Corrupting)
				{
					newInfo.CorruptionStartTime = DateTime.MinValue;
					// this will be tattled by corrupted check
				}
				else if (newInfo.Corrupting == Faction.None)
				{
					// it reset

					newInfo.CorruptionStartTime = DateTime.MinValue;

					var message = string.Format("{0} is no longer corrupting {1} and has reset!", savedCity.Corrupting, newInfo.CityName);
					writer.SendMessageToChannels(settings.Channels, actionQueue, message);
				}
				else
				{
					newInfo.ResetCorruptionWarnings();
					newInfo.CorruptionStartTime = DateTime.UtcNow;

					var message = string.Format("{0} has begun corrupting {1}!", newInfo.Corrupting, newInfo.CityName);
					writer.SendMessageToChannels(settings.Channels, actionQueue, message);
				}
			}

			if (newInfo.Corrupted != savedCity.Corrupted)
			{
				if (newInfo.Corrupted == Faction.None && newInfo.Faction == savedCity.Corrupted)
				{
					// it was captured

					newInfo.LastCaptureTime = DateTime.UtcNow;
					newInfo.ResetCaptureWarnings();

					var message = string.Format("{0} has captured {1}!", newInfo.Faction, newInfo.CityName);
					writer.SendMessageToChannels(settings.Channels, actionQueue, message);
				}
				else if (newInfo.Corrupted == Faction.None)
				{
					newInfo.LastCaptureTime = DateTime.MinValue;

					var message = string.Format("{1} is no longer corrupted by {0} and has reset!", savedCity.Corrupted, newInfo.CityName);
					writer.SendMessageToChannels(settings.Channels, actionQueue, message);
				}
				else
				{
					newInfo.CorruptionStartTime = DateTime.MinValue;

					var message = string.Format("{0} has corrupted {1}!", newInfo.Corrupted, newInfo.CityName);
					writer.SendMessageToChannels(settings.Channels, actionQueue, message);
				}
			}

			if (newInfo.Faction != savedCity.Faction)
			{
				// another faction captured

				newInfo.LastCaptureTime = DateTime.UtcNow;
				newInfo.ResetCaptureWarnings();

				var message = string.Format("{0} has captured {1}!", newInfo.Faction, newInfo.CityName);
				writer.SendMessageToChannels(settings.Channels, actionQueue, message);
			}
		}

		public void UpdateSigilInfo(StreamWriter writer, Queue<Task> actionQueue, Settings settings)
		{
			var sigilInfos = this.GetAllSigilInfo();

			if (sigilInfos == null) return;

			foreach (var newInfo in sigilInfos)
			{
				var savedCity = settings.SigilStatus
					.SingleOrDefault(x => x.CityName.Equals(newInfo.CityName, StringComparison.OrdinalIgnoreCase));

				if (savedCity != null)
				{
					newInfo.CorruptionStartTime = savedCity.CorruptionStartTime;
					newInfo.LastCaptureTime = savedCity.LastCaptureTime;
					newInfo.WarnedCorruptionHalfHourReset = savedCity.WarnedCorruptionHalfHourReset;
					newInfo.WarnedCorruptionTenMinuteReset = savedCity.WarnedCorruptionTenMinuteReset;
					newInfo.WarnedFiveHoursReset = savedCity.WarnedFiveHoursReset;
					newInfo.WarnedHourReset = savedCity.WarnedHourReset;
					newInfo.WarnedHalfHourReset = savedCity.WarnedHalfHourReset;
					newInfo.WarnedReset = savedCity.WarnedReset;

					this.TattleOnSigilActivity(newInfo, savedCity, writer, actionQueue, settings);
				}

				if (savedCity == null)
				{
					settings.SigilStatus.Add(newInfo);
					continue;
				}
				else
				{
					settings.SigilStatus.Remove(savedCity);
					settings.SigilStatus.Add(newInfo);
				}
			}
		}
	}
}
