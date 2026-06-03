using HarmonyLib;
using Pathea;
using Pathea.Behavior;
using Pathea.EG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityModManagerNet;

namespace FavorHelper
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Enabled;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        [HarmonyPatch(typeof(EGDate), "Update")]
        public static class EGDate_Update_ForceLock_Patch
        {
            public static void Postfix(EGDate __instance)
            {
                if (!Enabled) return;

                var traverse = Traverse.Create(__instance);
                int forceMax = traverse.Field("mForceMax").GetValue<int>();

                traverse.Field("mForce").SetValue(forceMax);

                var forceValue = traverse.Field("mForceValue").GetValue<BehaviorDesigner.Runtime.SharedInt>();
                if (forceValue != null)
                {
                    forceValue.Value = forceMax;
                }
            }
        }

        [HarmonyPatch(typeof(EGData), "GetEventCount")]
        public static class EGData_GetEventCount_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!Main.Enabled) return true;

                __result = 100;

                return false;
            }
        }

        [HarmonyPatch(typeof(EGMgr), "IsEngagementReady")] // 重複玩耍與約會
        static class EGMgr_IsEngagementReady_Patch
        {
            static void Postfix(EGMgr __instance, ref bool __result)
            {
                if (!Main.Enabled) return;

                var mDate = Traverse.Create(__instance).Field("mDate").GetValue<Pathea.EG.EGDate>();

                if (mDate == null)
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(Player), "CanParty", new Type[] { })] //重複宴會
        public static class Player_CanParty_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (!Enabled) return;

                __result = true;
            }
        }
    }
}
