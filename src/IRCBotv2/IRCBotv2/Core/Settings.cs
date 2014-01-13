using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace IRCBotv2.Core
{
	public class Settings
	{
		public string User { get; set; }

		public string Nick { get; set; }

		public string Server { get; set; }

		public int Port { get; set; }

		public List<Channel> Channels { get; set; }

		public List<string> AuthenticatedHostnames { get; set; }

		public List<string> FriendHostnames { get; set; }

		public List<SigilInfo> SigilStatus { get; set; }

		public Dictionary<FullName, DateTime> BodReminderList { get; set; }

		private const string DateTimeDefault = "0001-01-01T00:00:00";

		public void LoadFromFile(string filename)
		{
			string data;
			using (var r = new StreamReader(filename))
			{
				data = r.ReadToEnd();
			}

			var settingsObj = JsonConvert.DeserializeObject<Settings>(data);

			this.User = settingsObj.User;
			this.Nick = settingsObj.Nick;
			this.Server = settingsObj.Server;
			this.Port = settingsObj.Port;
			this.Channels = settingsObj.Channels ?? new List<Channel>();
			this.FriendHostnames = settingsObj.FriendHostnames ?? new List<string>();
			this.AuthenticatedHostnames = settingsObj.AuthenticatedHostnames ?? new List<string>();
			this.SigilStatus = settingsObj.SigilStatus ?? new List<SigilInfo>();
			this.BodReminderList = settingsObj.BodReminderList ?? new Dictionary<FullName, DateTime>();

			foreach (var sigil in this.SigilStatus)
			{
				if (sigil.LastCaptureTime == DateTime.Parse(DateTimeDefault))
				{
					sigil.LastCaptureTime = DateTime.MinValue;
				}

				if (sigil.CorruptionStartTime == DateTime.Parse(DateTimeDefault))
				{
					sigil.CorruptionStartTime = DateTime.MinValue;
				}
			}
		}

		public void SaveToFile(string filename)
		{
			var settingsString = JsonConvert.SerializeObject(this, Formatting.Indented);

			if (File.Exists(filename))
			{
				File.Delete(filename);
			}

			using (var w = new StreamWriter(filename))
			{
				w.Write(settingsString);
			}
		}
	}
}
