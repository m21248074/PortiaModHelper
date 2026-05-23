using System;
using Harmony12;
using Pathea.ModuleNs;
using Pathea.TreasureRevealerNs;
using RelicManager.Core;

namespace RelicManager.Patches
{
	// Token: 0x0200000C RID: 12
	[HarmonyPatch(typeof(TreasureRevealer), "EnterAimMode")]
	internal class TreasureRevealer_EnterAimMode_Patch
	{
		// Token: 0x06000029 RID: 41 RVA: 0x00003CC1 File Offset: 0x00001EC1
		[HarmonyPrefix]
		private static void Prefix()
		{
			if (!Main.enabled || !Controller.Instance.ShowNamesEnabled)
			{
				Module<TreasureRevealerManager>.Self.property.showName = false;
				return;
			}
			Module<TreasureRevealerManager>.Self.property.showName = true;
		}
	}
}
