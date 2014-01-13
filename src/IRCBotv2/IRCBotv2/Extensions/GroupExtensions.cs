using System.Text.RegularExpressions;

namespace IRCBotv2.Extensions
{
	public static class GroupExtensions
	{
		public static string StringValue(this Group group)
		{
			return group.Success ? group.Value : string.Empty;
		}
	}
}
