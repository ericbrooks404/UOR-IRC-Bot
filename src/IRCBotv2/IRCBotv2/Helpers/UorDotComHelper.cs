using IRCBotv2.Enums;

namespace IRCBotv2.Helpers
{
	public class UorDotComHelper
	{
		public static Faction GetFaction(string s)
		{
			switch (s)
			{
				case "council of mages":
					return Faction.CouncilOfMages;
				case "true britannians":
					return Faction.TrueBritannians;
				case "shadowlords":
					return Faction.Shadowlords;
				case "minax":
					return Faction.Minax;
				default:
					return Faction.None;
			}
		}
	}
}
