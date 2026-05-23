using System;
using Pathea;
using Pathea.HomeNs;
using Pathea.ModuleNs;

namespace RelicManager.Modules
{
	// Token: 0x0200000F RID: 15
	public class ItemRetriever
	{
		// Token: 0x06000030 RID: 48 RVA: 0x00003DF4 File Offset: 0x00001FF4
		public static int GetOwnedByID(int id)
		{
			int num = 0;
			for (int i = 0; i < StorageUnit.GlobalCount; i++)
			{
				num += StorageUnit.GetStorageByGlobalIndex(i).Storeage.GetItemCount(id);
			}
			return num + Module<Player>.Self.bag.GetItemCount(id);
		}
	}
}
