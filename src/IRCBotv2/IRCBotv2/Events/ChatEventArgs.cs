using System;

namespace IRCBotv2.Events
{
	public class ChatEventArgs : EventArgs
	{
		public string Input { get; set; }

		public ChatEventArgs(string input)
		{
			this.Input = input;
		}
	}
}
