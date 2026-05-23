using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Harmony12;
using Pathea.TreasureRevealerNs;
using RelicManager.Core;
using RelicManager.Modules;

namespace RelicManager.Patches
{
	// Token: 0x0200000A RID: 10
	[HarmonyPatch(typeof(TreasureRevealerItem), "Init")]
	internal static class TreasureRevealerItem_Init_Patch
	{
		// Token: 0x06000025 RID: 37 RVA: 0x00003BD8 File Offset: 0x00001DD8
		[HarmonyPrefix]
		private static void Prefix(ref string name, out bool __state)
		{
			string temp = Regex.Replace(name, "\\sPiece\\s\\d+", "", RegexOptions.IgnoreCase);
			Relic relic2 = Relics.RelicsList.SingleOrDefault((Relic relic) => relic.Name.Equals(temp));
			if (relic2 != null)
			{
				int num = int.Parse(Regex.Replace(name, "\\D", ""));
				name = string.Format("{0}\nOwned: {1}", name, ItemRetriever.GetOwnedByID(relic2.PieceIDs[num - 1]));
				__state = true;
				return;
			}
			__state = false;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00003C60 File Offset: 0x00001E60
		[HarmonyPostfix]
		private static void Postfix(TreasureRevealerItem __instance, bool __state)
		{
			if (__state)
			{
				List<TreasureRevealerItem> treasures = Controller.Instance.treasures;
				lock (treasures)
				{
					Controller.Instance.treasures.Add(__instance);
				}
			}
		}
	}
}
