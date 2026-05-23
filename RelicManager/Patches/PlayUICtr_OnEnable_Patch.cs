using System;
using Harmony12;
using Pathea.UISystemNs.PlayUI;
using RelicManager.Modules;
using UnityEngine;

namespace RelicManager.Patches
{
	// Token: 0x02000009 RID: 9
	[HarmonyPatch(typeof(PlayUICtr), "OnEnable")]
	internal static class PlayUICtr_OnEnable_Patch
	{
		// Token: 0x06000024 RID: 36 RVA: 0x00003BAC File Offset: 0x00001DAC
		[HarmonyPostfix]
		private static void PostFix()
		{
			PlayUICtr playUICtr = UnityEngine.Object.FindObjectOfType<PlayUICtr>();
			if (playUICtr.gameObject.GetComponent<GuiActivator>() == null)
			{
				playUICtr.gameObject.AddComponent<GuiActivator>();
			}
		}
	}
}
