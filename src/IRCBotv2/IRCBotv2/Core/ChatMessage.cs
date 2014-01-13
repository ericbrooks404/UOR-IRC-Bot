using System;
using System.Text.RegularExpressions;
using IRCBotv2.Enums;
using IRCBotv2.Extensions;

namespace IRCBotv2.Core
{
	public class ChatMessage
	{
		public string RawMessage { get; set; }

		public FullName Sender { get; set; }

		public string Action { get; set; }

		public FullName Recipient { get; set; }

		public string Message { get; set; }

		public MessageSenderType MessageSenderType { get; set; }

		public ChatMessage(string rawMessage, 
							string sender, 
							string action, 
							string recipient, 
							string message, 
							MessageSenderType messageSenderType)
		{
			this.RawMessage = rawMessage;
			this.Sender = sender.GetFullName();
			this.Action = action;
			this.Recipient = recipient.GetFullName();
			this.Message = message;
			this.MessageSenderType = messageSenderType;
		}

		public ChatMessage(GroupCollection groups, string nick)
		{
			this.RawMessage = groups[0].StringValue();

			this.Sender = (groups[1].StringValue() == string.Empty) 
									? new FullName()
									: groups[1].StringValue().GetFullName();

			this.Action = groups[2].StringValue();

			this.Recipient = string.IsNullOrWhiteSpace(groups[3].StringValue())
									? new FullName()
									: groups[3].StringValue().GetFullName();

			this.Message = groups[4].StringValue();

			this.MessageSenderType = this.GetMessageSenderType(nick);
		}

		private MessageSenderType GetMessageSenderType(string nick)
		{
			if (string.IsNullOrWhiteSpace(this.Recipient.Nick)
				&& nick.Equals(this.Sender.RawData))
			{
				return MessageSenderType.Myself;
			}
			else if (string.IsNullOrWhiteSpace(this.Sender.Hostname)
				&& string.IsNullOrWhiteSpace(this.Sender.Username))
			{
				return MessageSenderType.Server;
			}
			else if (!string.IsNullOrWhiteSpace(this.Sender.Hostname)
				&& this.Sender.Nick.Equals(this.Recipient.Nick, StringComparison.OrdinalIgnoreCase))
			{
				return MessageSenderType.Myself;
			}
			else if (this.MessageSenderType == MessageSenderType.None
				&& !string.IsNullOrEmpty(this.Sender.Hostname))
			{
				return MessageSenderType.Other;
			}
			else
			{
				return MessageSenderType.None;
			}
		}
	}
}
