using System;
using System.IO;
using System.Reflection;
using Harmony12;
using RelicManager.Core;
using UnityEngine;
using UnityModManagerNet;

namespace RelicManager
{
	// Token: 0x02000004 RID: 4
	internal static class Main
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00003015 File Offset: 0x00001215
		private static string KeybindTextColor
		{
			get
			{
				if (!Main.IsChangingKey)
				{
					return "White";
				}
				return "Yellow";
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x0000302C File Offset: 0x0000122C
		private static bool Load(UnityModManager.ModEntry modEntry)
		{
			Main.Logger = modEntry.Logger;
			modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(Main.OnToggle);
			modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGui);
			Main.keyConfPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\keybind.txt";
			Main.keyCodes = Enum.GetValues(typeof(KeyCode));
			Main.IsChangingKey = false;
			try
			{
				Main.harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
				Main.harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
			}
			catch (Exception ex)
			{
				Main.Logger.LogException(ex);
			}
			return true;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000030E8 File Offset: 0x000012E8
		private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
		{
			Main.enabled = value;
			return true;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000030F4 File Offset: 0x000012F4
		private static void OnGui(UnityModManager.ModEntry modEntry)
		{
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Relic manager open/close: ", new GUILayoutOption[0]);
			if (GUILayout.Button(string.Concat(new string[]
			{
				"<color=",
				Main.KeybindTextColor,
				">",
				Enum.GetName(typeof(KeyCode), Controller.Instance.KeyBind),
				"</color>"
			}), new GUILayoutOption[0]))
			{
				Main.IsChangingKey = true;
			}
			if (Main.IsChangingKey)
			{
				foreach (object obj in Main.keyCodes)
				{
					KeyCode keyCode = (KeyCode)obj;
					if (Input.GetKeyUp(keyCode) && keyCode != KeyCode.Mouse0)
					{
						Controller.Instance.KeyBind = keyCode;
						Main.IsChangingKey = false;
						break;
					}
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			Controller.Instance.ShowNamesEnabled = GUILayout.Toggle(Controller.Instance.ShowNamesEnabled, "Show names on scanner", new GUILayoutOption[0]);
			GUILayout.EndVertical();
		}

		// Token: 0x0400000C RID: 12
		public static HarmonyInstance harmonyInstance;

		// Token: 0x0400000D RID: 13
		public static UnityModManager.ModEntry.ModLogger Logger;

		// Token: 0x0400000E RID: 14
		public static bool enabled;

		// Token: 0x0400000F RID: 15
		public static string keyConfPath;

		// Token: 0x04000010 RID: 16
		private static Array keyCodes;

		// Token: 0x04000011 RID: 17
		private static bool IsChangingKey;
	}
}
