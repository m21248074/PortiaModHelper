using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityModManagerNet;
using HarmonyLib;
using Pathea.CaiQuanNs;

namespace CaiQuanMaster
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Enabled = true;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;

            var harmony = new HarmonyLib.Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        [HarmonyPatch(typeof(CaiQuanGame), "StartAnim")]
        public class CaiQuanGame_StartAnim_Patch
        {
            public static void Prefix(CaiQuanType playerType, ref CaiQuanType npcType)
            {
                if (!Enabled) return;

                if (playerType == CaiQuanType.Jiandao) npcType = CaiQuanType.Bu;
                else if (playerType == CaiQuanType.Shitou) npcType = CaiQuanType.Jiandao;
                else if (playerType == CaiQuanType.Bu) npcType = CaiQuanType.Shitou;
            }
        }

        [HarmonyPatch(typeof(CaiQuanGame), "CheckWinner")]
        public class CaiQuanGame_CheckWinner_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!Enabled) return true;

                __result = 0;
                return false;
            }
        }

    }
}
