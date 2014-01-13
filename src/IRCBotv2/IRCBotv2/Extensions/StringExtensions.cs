using IRCBotv2.Core;
using System.Text.RegularExpressions;

namespace IRCBotv2.Extensions
{
	public static class StringExtensions
	{
		public static FullName GetFullName(this string s)
		{
			var fullName = new FullName();
			var nameRegex = new Regex(@"([^!]*?)!([^@]*?)@([^\s]*)");
			var nameMatch = nameRegex.Match(s);

			fullName.RawData = s;
			fullName.Nick = nameMatch.Groups[1].StringValue();
			fullName.Username = nameMatch.Groups[2].StringValue();
			fullName.Hostname = nameMatch.Groups[3].StringValue();

			return fullName;
		}
	}
}
