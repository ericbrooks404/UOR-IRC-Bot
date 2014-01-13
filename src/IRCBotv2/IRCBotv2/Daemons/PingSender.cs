using System;
using System.IO;
using System.Threading;

/*
* Class that sends PING to irc server every 15 seconds
*/
namespace IRCBotv2.Daemons
{
	internal class PingSender
	{
		private const string Ping = "PING :";
		private readonly Thread _pingSender;

		public PingSender(StreamWriter writer, string server)
		{
			_pingSender = new Thread(() => Run(writer, server))
				{
					IsBackground = true
				};
		}

		public void Start()
		{
			_pingSender.Start();
		}

		public void Stop()
		{
			_pingSender.Abort();
		}

		public static void Run(StreamWriter writer, string server)
		{
			try
			{
				while (true)
				{
					writer.WriteLine(Ping + server);
					writer.Flush();
					Thread.Sleep(15000);
				}
			}
			catch (Exception e)
			{

			}
		}
	}
}