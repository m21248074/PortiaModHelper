using HarmonyLib;
using Pathea.MiniGameNs;
using System;
using System.Reflection;
using UnityModManagerNet;

namespace CatchCowMaster
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

        [HarmonyPatch(typeof(AnimalBehavior), "Update")]
        public class AnimalBehavior_Update_Patch
        {
            public static bool Prefix(AnimalBehavior __instance)
            {
                if (!Enabled) return true;

                var traverse = Traverse.Create(__instance);
                Animal curRunAnimal = traverse.Field("curRunAnimal").GetValue<Animal>();

                if (curRunAnimal != null)
                {
                    MethodInfo hasReachedMethod = __instance.GetType().GetMethod("HasReached", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (hasReachedMethod != null)
                    {
                        bool hasReached = (bool)hasReachedMethod.Invoke(__instance, new object[] { curRunAnimal });

                        if (hasReached)
                        {
                            int trackIndex = traverse.Field("trackIndex").GetValue<int>();

                            if (__instance.CatcheSucceed != null)
                            {
                                __instance.CatcheSucceed(trackIndex, curRunAnimal);
                            }

                            CatchCowGameCtr gameCtr = UnityEngine.Object.FindObjectOfType<CatchCowGameCtr>();
                            if (gameCtr != null)
                            {
                                try
                                {
                                    MethodInfo showLeftCircleMethod = gameCtr.GetType().GetMethod("ShowLeftCircle", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (showLeftCircleMethod != null)
                                    {
                                        showLeftCircleMethod.Invoke(gameCtr, null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Main.Logger.Error($"強制扣除繩子失敗: {ex.Message}");
                                }
                            }

                            __instance.DestroyAnimal();
                            __instance.CreateAnimal();

                            Main.Logger.Log("[Catch Cow Hacker] 動物出界，強制執行：成功了！！");
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(AnimalBehavior), "InitAnimal")]
        public class AnimalBehavior_InitAnimal_Patch
        {
            public static void Postfix(AnimalBehavior __instance)
            {
                if (!Enabled) return;

                var traverse = Traverse.Create(__instance);
                Animal curRunAnimal = traverse.Field("curRunAnimal").GetValue<Animal>();
                if (curRunAnimal != null)
                {
                    float baseSpeed = curRunAnimal.Attrib.RunBaseSpeed;

                    float hyperSpeed = baseSpeed * 10f;

                    curRunAnimal.SetMoveSpeed(hyperSpeed);

                    Main.Logger.Log($"[Catch Cow Hacker] 動物出生！速度已由 {baseSpeed} 提升至瘋狂的 {hyperSpeed}！");
                }
            }
        }
    }
}
