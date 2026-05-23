using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Harmony12;
using Pathea.ItemSystem;
using Pathea.TreasureRevealerNs;
using RelicManager.Core;
using RelicManager.Modules;

namespace RelicManager.Patches
{
	// Token: 0x02000006 RID: 6
	[HarmonyPatch(typeof(ItemBag), "AddItem", new Type[]
	{
		typeof(ItemObject),
		typeof(bool),
		typeof(AddItemMode)
	})]
	internal class ItemBag_AddItem_Patch
	{
		// Token: 0x06000020 RID: 32 RVA: 0x00003884 File Offset: 0x00001A84
		[HarmonyPostfix]
		private static void Postfix(ItemObject item)
		{
			if (!Main.enabled || !Controller.Instance.IsTreasureAwake || !item.ItemBase.Name.Contains("Piece "))
			{
				return;
			}
			Relic relic2 = Relics.RelicsList.SingleOrDefault((Relic relic) => item.ItemBase.Name.Contains(relic.Name));
			if (relic2 != null)
			{
				int num = int.Parse(Regex.Match(item.ItemBase.Name, "\\d+").Value);
				int ownedByID = ItemRetriever.GetOwnedByID(relic2.PieceIDs[num - 1]);
				List<TreasureRevealerItem> treasures = Controller.Instance.treasures;
				lock (treasures)
				{
					foreach (TreasureRevealerItem treasureRevealerItem in Controller.Instance.treasures)
					{
						if (!(treasureRevealerItem == null))
						{
							Traverse traverse = Traverse.Create(treasureRevealerItem).Field("mItemLines").Property("itemName", null);
							string value = traverse.GetValue<string>();
							if (value.Contains(item.ItemBase.Name))
							{
								traverse.SetValue(Regex.Replace(value, "Owned: \\d+", string.Format("Owned: {0}", ownedByID)));
							}
						}
					}
				}
			}
		}
	}
}
