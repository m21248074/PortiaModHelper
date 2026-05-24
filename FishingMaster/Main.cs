using HarmonyLib;
using Pathea;
using Pathea.ItemSystem;
using Pathea.MiniGameNs;
using Pathea.ModuleNs;
using Pathea.MiniGameNs.Fishing;
using System.Collections.Generic;
using System.Reflection;
using UnityModManagerNet;
using System;

namespace FishingMaster
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

        [HarmonyPatch(typeof(Rub_FishDropList), "GetRand")]
        public static class Rub_FishDropList_GetRand_Patch
        {
            public static bool Prefix(Rub_FishDropList __instance, List<int> abandonedItemId, ref bool isrub, ref int __result)
            {
                if (!Main.Enabled) return true;

                try
                {
                    ItemObject item = Module<Player>.Self.GetHoldItem();
                    int poleId = item.ItemDataId;

                    float currentRubRate = (poleId == 1005003) ? 0f : __instance.rubRate;

                    if (currentRubRate > 0f && UnityEngine.Random.value <= currentRubRate)
                    {
                        isrub = true;
                        List<DoubleInt> idWeightList = __instance.rubWeightList.FindAll((DoubleInt it) => !abandonedItemId.Contains(it.id0));
                        __result = MathUtils.RandomPickInt(idWeightList);
                        return false;
                    }
                    isrub = false;
                    List<DoubleInt> idWeightList2 = __instance.fishWeightList.FindAll((DoubleInt it) => !abandonedItemId.Contains(it.id0));

                    for (int i = 0; i < idWeightList2.Count; i++)
                    {
                        if (!idWeightList2[i].id0.ToString().StartsWith("400")) continue;

                        int newWeight = -1;
                        int oldWeight = idWeightList2[i].id1;

                        if (poleId == 1005002)
                            newWeight = (oldWeight == 200) ? 110 : (oldWeight == 20) ? 110 : oldWeight;
                        else if (poleId == 1005003)
                            newWeight = (oldWeight == 200) ? 0 : (oldWeight == 20) ? 200 : (oldWeight == 5) ? 25 : oldWeight;

                        if (newWeight != -1)
                            idWeightList2[i] = new DoubleInt(idWeightList2[i].id0, newWeight);
                    }

                    __result = MathUtils.RandomPickInt(idWeightList2);
                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(FishingSystem_t), "UpdateAimPos")]
        public static class FishingSystem_t_UpdateAimPos_Patch
        {
            public static bool Prefix(FishingSystem_t __instance, float delta, bool isJoystick)
            {
                if (!Main.Enabled) return true;

                try
                {
                    BindingFlags privateFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                    Type sysType = typeof(FishingSystem_t);

                    FieldInfo fieldCurFish = sysType.GetField("curFish", privateFlags);
                    FieldInfo fieldAimObj = sysType.GetField("aimObj", privateFlags);
                    if (fieldCurFish == null) return true;
                    Fish_t curFish = fieldCurFish.GetValue(__instance) as Fish_t;
                    object aimObj = fieldAimObj.GetValue(__instance);

                    if (curFish == null || aimObj == null) return true;

                    PropertyInfo propPos = aimObj.GetType().GetProperty("Pos", BindingFlags.Public | BindingFlags.Instance);
                    if (propPos != null && propPos.CanWrite)
                    {
                        propPos.SetValue(aimObj, curFish.CurPostion, null);
                    }

                    MethodInfo methodGetFishAngle = sysType.GetMethod("GetFishAngle", privateFlags);
                    FieldInfo fieldCurPlayerAngle = sysType.GetField("curPlayerAngle", privateFlags);

                    if (methodGetFishAngle != null && fieldCurPlayerAngle != null)
                    {
                        float fishAngle = (float)methodGetFishAngle.Invoke(__instance, null);
                        fieldCurPlayerAngle.SetValue(__instance, fishAngle);
                    }

                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }
    }
}
