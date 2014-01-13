using IRCBotv2.ActionHandlers;
using IRCBotv2.Core;
using IRCBotv2.Daemons;
using IRCBotv2.Enums;
using IRCBotv2.Events;
using IRCBotv2.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IrcBotV2.Core
{
	public partial class IrcBotV2 : IDisposable
	{
		private Settings Settings { get; set; }

		private Queue<Task> ActionQueue { get; set; }

		private List<StatusUpdate> StatusList { get; set; }

		private readonly Regex _messageRegex = new Regex(@"^(?:[:](\S+) )?(\S+)(?: (?!:)(.+?))?(?: [:](.+))?$");

		private StreamWriter Writer { get; set; }

		private StreamReader Reader { get; set; }

		private NetworkStream Stream { get; set; }

		private DateTime Uptime { get; set; }

		private Thread IdentServiceThread { get; set; }

		private Thread QueueProcessorThread { get; set; }

		private PingSender PingSender { get; set; }

		public delegate void ChatEventHandler(object sender, ChatEventArgs args);

		public event ChatEventHandler OnChat;

		public bool RecievedDie { get; set; }

		public SigilsService SigilsService { get; set; }

		public StatusService StatusService { get; set; }
		
		// control flow
		public IrcBotV2()
		{
			Initialize();
			InitializeDaemons();
			InitializeEvents();

			this.Listen();

			this.Dispose();
		}

		private void InitializeEvents()
		{
			this.OnChat += this.ParseInput;

			this.InitializeHeartBeatEvents();
		}

		// driver method
		public void Listen()
		{
			while (!this.RecievedDie)
			{
				//try
				//{
					string input;
					while (!this.RecievedDie && (input = this.Reader.ReadLine()) != null)
					{
						if (DateTime.Compare(DateTime.UtcNow, this.NextHeartbeatTime) > 0)
						{
							this.NextHeartbeatTime = DateTime.UtcNow.Add(this.HeartbeatFrequency);

							// fire the heartbeat event
							this.OnHeartbeat(this, new EventArgs());
						}

						var eventArgs = new ChatEventArgs(input);

						// fire a chat event if new input from the server happens
						this.OnChat(this, eventArgs);
					}
				//}
				//catch (Exception e)
				//{
					//Console.WriteLine("-- Exception encountered: {0}", e.Message);

					//this.Dispose();

					//Thread.Sleep(1000);

					//Initialize(true);
					//InitializeDaemons();
					//InitializeEvents();
				//}
			}
		}

		private void Initialize(bool reconnect = false, int tries = 0)
		{
			this.LoadSettings();

			try
			{
				// init IRC connection
				var client = new TcpClient(this.Settings.Server, this.Settings.Port);
				this.Stream = client.GetStream();
				this.Reader = new StreamReader(this.Stream);
				this.Writer = new StreamWriter(this.Stream)
				{
					AutoFlush = true,
					NewLine = "\r\n"
				};
			}
			catch (Exception)
			{
				Console.WriteLine("- Failed to connect to server! ({0} tries)", ++tries);

				if (tries > 20)
				{
					Console.WriteLine("- Halting after {0} connection attempts.", tries);

					this.Dispose();
				}
				else
				{
					Thread.Sleep(60000);

					this.Initialize(true, tries);	
				}
			}

			if (!reconnect)
			{
				// set initial values for instance properties
				this.ActionQueue = new Queue<Task>();
				this.StatusList = new List<StatusUpdate>();
				this.RecievedDie = false;

				// instantiate our data services
				this.SigilsService = new SigilsService();
				this.StatusService = new StatusService();	
			}

			this.CheckStatus(null, null);
			this.InitializeHeartbeat();
			this.Uptime = DateTime.UtcNow;
		}

		private void InitializeDaemons()
		{
			this.IdentServiceThread = new Thread(IdentService.Listen)
				{
					IsBackground = true
				};

			this.IdentServiceThread.Start();

			this.QueueProcessorThread = new Thread(() => QueueProcessor.Start(this.ActionQueue))
				{
					IsBackground = true
				};

			this.QueueProcessorThread.Start();

			this.PingSender = new PingSender(this.Writer, this.Settings.Server);
			this.PingSender.Start();
		}

		public void ParseInput(object sender, ChatEventArgs e)
		{
			if (this.ActionQueue.Count > 15) return;

			ChatMessage message;

			Console.WriteLine("< " + e.Input);

			var match = _messageRegex.Match(e.Input);

			if (match.Success)
			{
				message = new ChatMessage(match.Groups, this.Settings.Nick);
			}
			else
			{
				if (e.Input.StartsWith("PING"))
				{
					var serial = e.Input.Substring(e.Input.IndexOf(':'), 8);
					message = new ChatMessage(e.Input, this.Settings.Server, "ping", this.Settings.Nick, serial, MessageSenderType.Server);
				}
				else
				{
					return;
				}
			}

			this.ExtractTasks(message);
		}

		private void ExtractTasks(ChatMessage message)
		{
			if (message == null) return;

			List<Task> tasks;

			switch (message.Action.ToLower())
			{
				case "notice":
					tasks = NoticeHandler.GetTasks(this.Writer, message, this.Settings.Nick, this.Settings.User);
					break;
				case "ping":
					tasks = PingHandler.GetTasks(this.Writer, message);
					break;
				case "mode":
					tasks = ModeHandler.GetTasks(this.Writer, message, this.Settings.Channels);
					break;
				case "join":
					tasks = JoinHandler.GetTasks(this.Writer, message, this.Settings.FriendHostnames);
					break;
				//case "part":
				//	break;
				case "353": // who is in channel list
					tasks = ChanListHandler.GetTasks(this.Writer, message, this.Settings.Channels, this.Settings.Nick);
					break;
				case "privmsg": // chat or PM, depending on target
					tasks = PrivmsgHandler.GetTasks(this.Writer, 
													message,
													this.Settings, 
													new Task(() => this.RecievedDie = true),
													new Task(this.SaveSettings),
													new Task(() => this.SigilsService.UpdateSigilInfo(this.Writer, this.ActionQueue, this.Settings)),
													this.Uptime, 
													this.StatusList);
					break;
				default:
					tasks = new List<Task>();
					break;
			}

			tasks.ForEach(x => this.ActionQueue.Enqueue(x));
		}

		private void LoadSettings()
		{
			var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ircbot.json");
			if (!File.Exists(fileName))
			{
				throw new FileLoadException(@"%AppData%\ircbot.json needs to exist more.");
			}

			this.Settings = new Settings();
			this.Settings.LoadFromFile(fileName);
		}

		private void SaveSettings()
		{
			var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ircbot.json");

			this.Settings.SaveToFile(fileName);
		}

		private void StopServices()
		{
			this.PingSender.Stop();

			this.IdentServiceThread.Abort();

			this.QueueProcessorThread.Abort();
		}

		public void Dispose()
		{
			this.SaveSettings();
			this.StopHeartBeat();
			this.StopServices();
			//this.Writer.Dispose();
			this.Reader.Dispose();
			this.Stream.Dispose();
		}
	}
}
