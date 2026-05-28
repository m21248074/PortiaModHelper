using HarmonyLib;
using Pathea.StoreNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityModManagerNet;

namespace RichShop
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Enabled = true;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        [HarmonyPatch(typeof(Store), "GenOwnMoney")]
        public class Store_GenOwnMoney_Patch
        {
            public static void Postfix(Store __instance)
            {
                if (!Enabled) return;

                var traverse = Traverse.Create(__instance);

                int currentMoney = traverse.Field("ownMoney").GetValue<int>();

                const int money = 100000;

                traverse.Field("ownMoney").SetValue(currentMoney + money);
            }
        }
    }
}
