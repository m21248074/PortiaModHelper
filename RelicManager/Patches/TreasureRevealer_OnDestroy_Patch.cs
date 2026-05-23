using System;
using System.Collections.Generic;
using Harmony12;
using Pathea.TreasureRevealerNs;
using RelicManager.Core;

namespace RelicManager.Patches
{
	// Token: 0x0200000D RID: 13
	[HarmonyPatch(typeof(TreasureRevealer), "OnDestroy")]
	internal class TreasureRevealer_OnDestroy_Patch
	{
		// Token: 0x0600002B RID: 43 RVA: 0x00003D00 File Offset: 0x00001F00
		[HarmonyPostfix]
		private static void Postfix()
		{
			Controller.Instance.IsTreasureAwake = false;
			List<TreasureRevealerItem> treasures = Controller.Instance.treasures;
			lock (treasures)
			{
				Controller.Instance.treasures.Clear();
			}
		}
	}
}
