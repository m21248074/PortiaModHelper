using System;
using RelicManager.Core;
using RelicManager.UI;
using UnityEngine;

namespace RelicManager.Modules
{
	// Token: 0x0200000E RID: 14
	internal class GuiActivator : MonoBehaviour
	{
		// Token: 0x0600002D RID: 45 RVA: 0x00003D5C File Offset: 0x00001F5C
		private void Start()
		{
			this._relicTracker = base.gameObject.AddComponent<RelicManagerGui>();
			this._relicTracker.enabled = false;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003D7C File Offset: 0x00001F7C
		private void Update()
		{
			bool keyUp = Input.GetKeyUp(KeyCode.Escape);
			bool keyUp2 = Input.GetKeyUp(Controller.Instance.KeyBind);
			if (!Main.enabled)
			{
				return;
			}
			if ((keyUp && Controller.Instance.ConfirmEnabled) || keyUp2)
			{
				Controller.Instance.ConfirmEnabled = !Controller.Instance.ConfirmEnabled;
				this._relicTracker.enabled = Controller.Instance.ConfirmEnabled;
			}
		}

		// Token: 0x04000021 RID: 33
		protected RelicManagerGui _relicTracker;
	}
}
