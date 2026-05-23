using System;
using System.Linq;
using System.Text;
using Harmony12;
using Pathea.ItemSystem;

namespace RelicManager.Patches
{
	// Token: 0x02000007 RID: 7
	[HarmonyPatch(typeof(ItemObject), "GenerateDescription")]
	internal static class ItemObject_GenerateDesc_Patch
	{
		// Token: 0x06000022 RID: 34 RVA: 0x00003A04 File Offset: 0x00001C04
		[HarmonyPostfix]
		private static void Postfix(ref string __result, ItemObject __instance)
		{
			if (!Main.enabled)
			{
				return;
			}
			Relic relic2 = Relics.RelicsList.SingleOrDefault((Relic relic) => __instance.ItemBase.Name.Contains(relic.Name));
			if (relic2 != null && __result.Contains("<color=#F9D872FF>"))
			{
				StringBuilder stringBuilder = new StringBuilder(__result);
				stringBuilder.Insert(stringBuilder.ToString().IndexOf("<color=#F9D872FF>"), "In Museum: " + (relic2.InMuseum ? "<color=green>Yes</color>" : "<color=red>No</color>") + "\n\n");
				stringBuilder.Insert(stringBuilder.ToString().IndexOf("<color=#F9D872FF>"), "Owned: \n");
				relic2.GetPiecesOwnedCounts();
				for (int i = 0; i < relic2.Pieces; i++)
				{
					stringBuilder.Insert(stringBuilder.ToString().IndexOf("<color=#F9D872FF>"), string.Format("Piece {0}: {1}\n", i + 1, relic2.PiecesOwnedCounts[i]));
				}
				stringBuilder.Insert(stringBuilder.ToString().IndexOf("<color=#F9D872FF>"), "\n");
				__result = stringBuilder.ToString();
			}
		}
	}
}
