using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IRCBotv2.Helpers;

namespace IRCBotv2.Daemons
{
	public class IdentService
	{
		public static void Listen()
		{
			var localIp = InternetsHelper.GetLocalIpAddress();
			var endpoint = new IPEndPoint(IPAddress.Parse(localIp), 113);
			var listener = new TcpListener(endpoint);

			listener.Start();

			var bytes = new Byte[256];
			var data = string.Empty;

			// Enter the listening loop. 
			while (string.IsNullOrEmpty(data))
			{
				Console.WriteLine("Ident Service: Waiting for a connection... ");

				TcpClient client = listener.AcceptTcpClient();
				Console.WriteLine("Connected!");

				NetworkStream stream = client.GetStream();

				int i;

				while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
				{
					data = Encoding.ASCII.GetString(bytes, 0, i);
					Console.WriteLine("Ident Service: Received: {0}", data);

					data = data.ToUpper();

					var split = data.Split(',');
					var localPort = split[0];
					var remotePort = split[1];

					var message = string.Format("{0}, 6667 : USERID : UNIX : {1}", localPort, "PvPBot");

					byte[] msg = Encoding.ASCII.GetBytes(message);

					stream.Write(msg, 0, msg.Length);
					Console.WriteLine("Ident Service: Sent: {0}", data);
				}

				client.Close();
			}
		}
	}
}