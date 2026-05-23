using System;
using Harmony12;
using Pathea.TreasureRevealerNs;
using RelicManager.Core;

namespace RelicManager.Patches
{
	// Token: 0x0200000B RID: 11
	[HarmonyPatch(typeof(TreasureRevealer), "Awake")]
	internal class TreasureRevealer_Awake_Patch
	{
		// Token: 0x06000027 RID: 39 RVA: 0x00003CAC File Offset: 0x00001EAC
		[HarmonyPostfix]
		private static void Postfix()
		{
			Controller.Instance.IsTreasureAwake = true;
		}
	}
}
