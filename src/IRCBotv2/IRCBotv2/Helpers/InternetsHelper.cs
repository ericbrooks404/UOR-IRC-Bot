using System.Net;
using System.Net.Sockets;

namespace IRCBotv2.Helpers
{
	class InternetsHelper
	{
		public static string GetLocalIpAddress()
		{
			IPHostEntry host;
			var localIp = "";
			host = Dns.GetHostEntry(Dns.GetHostName());

			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily != AddressFamily.InterNetwork) continue;

				localIp = ip.ToString();
				break;
			}

			return localIp;
		}
	}
}
