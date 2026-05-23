using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Pathea.Museum;

namespace RelicManager.Patches
{
	// Token: 0x02000008 RID: 8
	[HarmonyPatch(typeof(MuseumManager), "AddExhibit")]
	internal static class MuseumManager_AddExhibit_Patch
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00003B24 File Offset: 0x00001D24
		[HarmonyPostfix]
		private static void Postfix(ExhibitionSeatData seat)
		{
			Func<int, bool> cachedPredicate = null;
			foreach (Relic relic in Relics.RelicsList)
			{
				IEnumerable<int> baseIDs = relic.BaseIDs;
				Func<int, bool> predicate;
				if ((predicate = cachedPredicate) == null)
				{
					predicate = (cachedPredicate = ((int id) => seat.itemId == id));
				}
				if (baseIDs.Any(predicate))
				{
					relic.InMuseum = true;
				}
			}
		}
	}
}
