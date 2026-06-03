using HarmonyLib;
using Pathea.AchievementNs;
using Pathea.ModuleNs;
using System.Collections.Generic;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;

namespace AchivementHelper
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Enabled = true;

        private static Vector2 achScrollVector = Vector2.zero;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!Enabled) return;

            GUILayout.Space(5f);
            GUILayout.Space(10f);

            DrawAchievementViewer();
        }

        private static void DrawAchievementViewer()
        {
            if (Module<AchievementModule>.Self == null)
            {
                GUILayout.Label("<color=yellow>⚠️ Please load your saved game file first to initialize the Achievement Radar.</color>", new GUILayoutOption[0]);
                return;
            }

            var achModule = Module<AchievementModule>.Self;
            var traverse = Traverse.Create(achModule);

            var lockedList = traverse.Field("lockedAchievements").GetValue<List<AchievementModule.AchievementItem>>();
            var unlockedSet = traverse.Field("unlockedAchievements").GetValue<HashSet<int>>();

            if (lockedList == null || lockedList.Count == 0)
            {
                GUILayout.Label("<color=white>No achievements found or data error.</color>", new GUILayoutOption[0]);
                return;
            }

            GUILayout.Label($"<b>📋 Remaining Achievements to Chase: {lockedList.Count}</b>", new GUILayoutOption[0]);
            GUILayout.Space(5f);

            achScrollVector = GUILayout.BeginScrollView(achScrollVector, GUILayout.ExpandWidth(true), GUILayout.Height(450f));

            foreach (var item in lockedList)
            {
                string achName = achModule.GetAchievementName(item.id);
                string achDesc = achModule.GetAchievementDesc(item.id);

                string progressStr = item.checker != null ? item.checker.PrintString() : "0/1";

                GUILayout.BeginHorizontal("box");
                GUILayout.Label($"<color=#FF9800><b>[🔒 進行中]</b></color> <b>{achName}</b> (ID: {item.id})\n<color=#B0BEC5>{achDesc}</color>", GUILayout.Width(500f));
                GUILayout.FlexibleSpace();
                GUILayout.Label($"<color=#00E676><b>{progressStr}</b></color>", GUILayout.Width(150f));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }

    [HarmonyPatch(typeof(AchievementModule))]
    public static class AchievementModule_SpeedUp_Loop_Patch
    {
        [HarmonyPatch("TalkWithNpc")]
        [HarmonyPrefix]
        public static void TalkWithNpc_Prefix(AchievementModule __instance)
        {
            if (!Main.Enabled) return;

            for (int i = 0; i < 4; i++)
            {
                var updateAchievementMethod = Traverse.Create(__instance).Method(
                    "UpdateAchievement",
                    new object[] { AchievementType.NpcTalkTimes, UpdateType.Add, 1, 0 }
                );

                updateAchievementMethod.GetValue();
            }
        }

        [HarmonyPatch("GiftToNpc")]
        [HarmonyPrefix]
        public static void GiftToNpc_Prefix(AchievementModule __instance, bool isPositive, bool isLikely)
        {
            if (!Main.Enabled) return;

            var traverse = Traverse.Create(__instance);

            for (int i = 0; i < 4; i++)
            {
                traverse.Method("UpdateAchievement", new object[] { AchievementType.GiftTimes, UpdateType.Add, 1, 0 }).GetValue();

                if (isLikely)
                {
                    traverse.Method("UpdateAchievement", new object[] { AchievementType.GiftLikelyGoodsTimes, UpdateType.Add, 1, 0 }).GetValue();
                }
                if (!isPositive)
                {
                    traverse.Method("UpdateAchievement", new object[] { AchievementType.GiftBadGoodsTimes, UpdateType.Add, 1, 0 }).GetValue();
                }
            }
        }

        [HarmonyPatch("PickupItem")]
        [HarmonyPrefix]
        public static void PickupItem_Prefix(AchievementModule __instance, int itemId)
        {
            if (!Main.Enabled) return;

            for (int i = 0; i < 4; i++)
            {
                Traverse.Create(__instance).Method(
                    "UpdateAchievement",
                    new object[] { AchievementType.PickItemTimes, UpdateType.Add, 1, itemId }
                ).GetValue();
            }
        }
    }
}
