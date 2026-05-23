using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using Pathea.GameFlagNs;
using Pathea.TreasureRevealerNs;
using Pathea.UISystemNs;
using UnityEngine;

namespace RelicManager.Core
{
	// Token: 0x02000010 RID: 16
	public class Controller
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000032 RID: 50 RVA: 0x00003E43 File Offset: 0x00002043
		public static Controller Instance
		{
			get
			{
				Controller result;
				if ((result = Controller._instance) == null)
				{
					result = (Controller._instance = new Controller());
				}
				return result;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00003E59 File Offset: 0x00002059
		// (set) Token: 0x06000034 RID: 52 RVA: 0x00003E61 File Offset: 0x00002061
		public BoolTrue MouseLocker { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000035 RID: 53 RVA: 0x00003E6A File Offset: 0x0000206A
		// (set) Token: 0x06000036 RID: 54 RVA: 0x00003E74 File Offset: 0x00002074
		public bool IsTreasureAwake
		{
			get
			{
				return this._isTreasureAwake;
			}
			set
			{
				if (this._isTreasureAwake != value)
				{
					this._isTreasureAwake = value;
				}
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00003E8A File Offset: 0x0000208A
		// (set) Token: 0x06000038 RID: 56 RVA: 0x00003E94 File Offset: 0x00002094
		public List<TreasureRevealerItem> treasures
		{
			get
			{
				return this._treasures;
			}
			set
			{
				if (this._treasures != value)
				{
					this._treasures = value;
				}
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00003EAA File Offset: 0x000020AA
		// (set) Token: 0x0600003A RID: 58 RVA: 0x00003EB4 File Offset: 0x000020B4
		public KeyCode KeyBind
		{
			get
			{
				return this._keyBind;
			}
			set
			{
				if (value != this._keyBind)
				{
					try
					{
						File.WriteAllText(Main.keyConfPath, value.ToString());
					}
					catch (Exception ex)
					{
						Main.Logger.LogException(ex);
					}
					this._keyBind = value;
				}
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003F08 File Offset: 0x00002108
		public Controller()
		{
			this.ShowNamesEnabled = true;
			this.ConfirmEnabled = false;
			this.MouseLocker = new BoolTrue();
			this.IsTreasureAwake = false;
			this.treasures = new List<TreasureRevealerItem>();
			if (File.Exists(Main.keyConfPath))
			{
				try
				{
					this._keyBind = (KeyCode)Enum.Parse(typeof(KeyCode), File.ReadAllText(Main.keyConfPath));
					return;
				}
				catch (Exception ex)
				{
					Main.Logger.LogException(ex);
					this.KeyBind = KeyCode.PageUp;
					return;
				}
			}
			this.KeyBind = KeyCode.PageUp;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003FAC File Offset: 0x000021AC
		public void PauseGame()
		{
			UIStateComm.Instance.SetCursor(true);
			Singleton<GameFlag>.Instance.Add(Flag.Pause, this.MouseLocker);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003FCA File Offset: 0x000021CA
		public void ResumeGame()
		{
			UIStateComm.Instance.SetCursor(false);
			Singleton<GameFlag>.Instance.Remove(Flag.Pause, this.MouseLocker);
		}

		// Token: 0x04000022 RID: 34
		private static Controller _instance;

		// Token: 0x04000023 RID: 35
		private KeyCode _keyBind;

		// Token: 0x04000024 RID: 36
		private volatile bool _isTreasureAwake;

		// Token: 0x04000025 RID: 37
		private volatile List<TreasureRevealerItem> _treasures;

		// Token: 0x04000026 RID: 38
		public bool ConfirmEnabled;

		// Token: 0x04000028 RID: 40
		public bool ShowNamesEnabled;
	}
}
