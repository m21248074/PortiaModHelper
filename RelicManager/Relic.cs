using System;
using System.Linq;
using Pathea;
using Pathea.HomeNs;
using Pathea.ModuleNs;
using RelicManager.Modules;

namespace RelicManager
{
	// Token: 0x02000003 RID: 3
	public class Relic
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000005 RID: 5 RVA: 0x00002C6B File Offset: 0x00000E6B
		public string Name { get; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000006 RID: 6 RVA: 0x00002C73 File Offset: 0x00000E73
		public int Pieces { get; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000007 RID: 7 RVA: 0x00002C7B File Offset: 0x00000E7B
		public int DataDiscCost { get; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002C83 File Offset: 0x00000E83
		public double RestoreTime { get; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000009 RID: 9 RVA: 0x00002C8B File Offset: 0x00000E8B
		public string Location { get; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600000A RID: 10 RVA: 0x00002C93 File Offset: 0x00000E93
		// (set) Token: 0x0600000B RID: 11 RVA: 0x00002C9B File Offset: 0x00000E9B
		public bool InMuseum { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002CA4 File Offset: 0x00000EA4
		public int[] BaseIDs { get; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600000D RID: 13 RVA: 0x00002CAC File Offset: 0x00000EAC
		public int[] PieceIDs { get; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600000E RID: 14 RVA: 0x00002CB4 File Offset: 0x00000EB4
		// (set) Token: 0x0600000F RID: 15 RVA: 0x00002CBC File Offset: 0x00000EBC
		public int[] PiecesOwnedCounts { get; set; }

		// Token: 0x06000010 RID: 16 RVA: 0x00002CC8 File Offset: 0x00000EC8
		public Relic(string aName, int piecesNum, int discCostNum, double restoreTimeNum, string aLocation, int[] baseArr, int[] pieceArr)
		{
			this.Name = aName;
			this.Pieces = piecesNum;
			this.DataDiscCost = discCostNum;
			this.RestoreTime = restoreTimeNum;
			this.Location = aLocation;
			this.InMuseum = false;
			this.BaseIDs = baseArr;
			this.PieceIDs = pieceArr;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002D18 File Offset: 0x00000F18
		public int GetBaseOwned()
		{
			int num = 0;
			foreach (int id in this.BaseIDs)
			{
				num += ItemRetriever.GetOwnedByID(id);
			}
			return num;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002D4C File Offset: 0x00000F4C
		public void GetPiecesOwnedCounts()
		{
			this.PiecesOwnedCounts = new int[this.Pieces];
			for (int i = 0; i < this.Pieces; i++)
			{
				this.PiecesOwnedCounts[i] = ItemRetriever.GetOwnedByID(this.PieceIDs[i]);
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002D90 File Offset: 0x00000F90
		public void PullRelicPieces()
		{
			this.GetPiecesOwnedCounts();
			if (this.PiecesOwnedCounts.All((int num) => num == 0))
			{
				Main.Logger.Log(this.Name + " pull failed - no pieces present in bag or storage");
				return;
			}
			for (int i = 0; i < this.Pieces; i++)
			{
				if (this.PiecesOwnedCounts[i] != 0)
				{
					if (Module<Player>.Self.bag.GetItemCount(this.PieceIDs[i]) > 0)
					{
						Main.Logger.Log(string.Format("{0} Piece {1} pull failed - item already in bag", this.Name, i + 1));
					}
					else if (Module<Player>.Self.bag.GetFreeSlotCount(true) == 0)
					{
						Main.Logger.Log(string.Format("{0} Piece {1} pull failed - not enough space in bag", this.Name, i + 1));
					}
					else
					{
						for (int j = 0; j < StorageUnit.GlobalCount; j++)
						{
							StorageUnit storageByGlobalIndex = StorageUnit.GetStorageByGlobalIndex(j);
							if (storageByGlobalIndex.Storeage.GetItemCount(this.PieceIDs[i]) > 0)
							{
								Module<Player>.Self.bag.AddItem(this.PieceIDs[i], 1, false, 0);
								storageByGlobalIndex.Storeage.RemoveItemById(this.PieceIDs[i], 1);
								Main.Logger.Log(string.Format("{0} Piece {1} pull successful", this.Name, i + 1));
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002F08 File Offset: 0x00001108
		public void PullDataDiscs()
		{
			if (ItemRetriever.GetOwnedByID(Relics.DataDiscID) < this.DataDiscCost)
			{
				Main.Logger.Log(this.Name + " data disc pull failed - not enough discs");
				return;
			}
			bool itemCount = Module<Player>.Self.bag.GetItemCount(Relics.DataDiscID) != 0;
			int num = this.DataDiscCost;
			if (!itemCount && Module<Player>.Self.bag.GetFreeSlotCount(true) == 0)
			{
				Main.Logger.Log(this.Name + " data disc pull failed - not enough space in bag");
				return;
			}
			for (int i = 0; i < StorageUnit.GlobalCount; i++)
			{
				StorageUnit storageByGlobalIndex = StorageUnit.GetStorageByGlobalIndex(i);
				int itemCount2 = storageByGlobalIndex.Storeage.GetItemCount(Relics.DataDiscID);
				if (itemCount2 != 0)
				{
					for (int j = 0; j < itemCount2; j++)
					{
						if (num == 0)
						{
							Main.Logger.Log(this.Name + " data disc pull successful");
							return;
						}
						Module<Player>.Self.bag.AddItem(Relics.DataDiscID, 1, false, 0);
						storageByGlobalIndex.Storeage.RemoveItemById(Relics.DataDiscID, 1);
						num--;
					}
				}
			}
		}

		public static bool ExchangePieces(Relic sourceRelic, int sourcePieceIdx, Relic targetRelic, int targetPieceIdx)
		{
            if (sourceRelic == null || targetRelic == null)
            {
                Main.Logger.Log("【交換失敗】傳入的古物數據為空 (Null)!");
                return false;
            }

            if (sourcePieceIdx < 0 || sourcePieceIdx >= sourceRelic.Pieces || targetPieceIdx < 0 || targetPieceIdx >= targetRelic.Pieces)
            {
                Main.Logger.Log("【交換失敗】錯誤的碎片索引位置。");
                return false;
            }

            if (sourceRelic == targetRelic && sourcePieceIdx == targetPieceIdx)
            {
                Main.Logger.Log("【交換失敗】不能拿同一個碎片自己換自己。");
                return false;
            }

            int fromItemID = sourceRelic.PieceIDs[sourcePieceIdx];
            int toItemID = targetRelic.PieceIDs[targetPieceIdx];

            if (Module<Player>.Self.bag.GetItemCount(fromItemID) <= 0)
            {
                Main.Logger.Log(string.Format("【交換失敗】您的背包內目前沒有「{0}」的碎片 {1}!", sourceRelic.Name, sourcePieceIdx + 1));
                return false;
            }

            if (Module<Player>.Self.bag.GetItemCount(toItemID) <= 0 && Module<Player>.Self.bag.GetFreeSlotCount(true) <= 0)
            {
                Main.Logger.Log($"【交換失敗】背包空間不足！無法容納新古物「{targetRelic.Name}」的碎片。");
                return false;
            }

            Module<Player>.Self.bag.RemoveItem(fromItemID, 1, false);

            Module<Player>.Self.bag.AddItem(toItemID, 1, false, 0);

            Main.Logger.Log(string.Format("【交換成功】已成功消耗「{0}」碎片 {1}，換得「{2}」碎片 {3}",
                sourceRelic.Name, sourcePieceIdx + 1, targetRelic.Name, targetPieceIdx + 1));

            sourceRelic.GetPiecesOwnedCounts();
            if (sourceRelic != targetRelic)
            {
                targetRelic.GetPiecesOwnedCounts();
            }

            return true;
        }
	}
}
