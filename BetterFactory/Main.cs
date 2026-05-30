using HarmonyLib;
using Pathea;
using Pathea.CompoundSystem;
using Pathea.CreationFactory;
using Pathea.FarmFactoryNs;
using Pathea.FeatureNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace BetterFactory
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool ProductionInLoop = true;
        public bool PowerOnOutsideCtr = true;
        public bool FullWorktable = true;
        public bool GetFromProductionUI = true;
        public bool GetFromProductionUIUseMax = true;
        public bool ShowNotCraftableFromAssembly = true;
        public bool UseArtisanSkill = true;
        public bool Debug = true;
        public float CraftSpeedMult = 10f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Enabled = true;

        public static Settings _Settings { get; private set; }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            _Settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnSaveGUI = OnSaveGUI;
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

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            _Settings.Save(modEntry);
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            _Settings.ProductionInLoop = GUILayout.Toggle(_Settings.ProductionInLoop, "Automatically move factory production to Material Warehouse", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            _Settings.PowerOnOutsideCtr = GUILayout.Toggle(_Settings.PowerOnOutsideCtr, "Replace open Product Warehouse action by open Central Power Supply on external factory control panel.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            _Settings.GetFromProductionUI = GUILayout.Toggle(_Settings.GetFromProductionUI, "Add get button (instead of single query craft) to factory production UI. Used to get items from factory storage to plaer inventory.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            _Settings.GetFromProductionUIUseMax = GUILayout.Toggle(_Settings.GetFromProductionUIUseMax, "Set items count in \"Get\" dialog to max", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            _Settings.FullWorktable = GUILayout.Toggle(_Settings.FullWorktable, "Enable convert hardwood in wood in factory (exactly allow to craft \"NotAutomable\" but \"Instant\" items on Auto Worktable).", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            _Settings.ShowNotCraftableFromAssembly = GUILayout.Toggle(_Settings.ShowNotCraftableFromAssembly, "Show not craftable materials needed for Assembly Station on Auto Worktable. Useful for \"get\" option.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            _Settings.UseArtisanSkill = GUILayout.Toggle(_Settings.UseArtisanSkill, "Use Artisan Skill on Auto Worktable. Discount will be apply per individual item in query and rounded (not per full query, as on standard Worktable).", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            _Settings.Debug = GUILayout.Toggle(_Settings.Debug, "Write debug info to log.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Crafting speed multiplied by {0}", _Settings.CraftSpeedMult), new GUILayoutOption[0]);
            _Settings.CraftSpeedMult = GUILayout.HorizontalSlider(_Settings.CraftSpeedMult, 1f, 100f, new GUILayoutOption[0]);
            _Settings.CraftSpeedMult = (float)Math.Round((double)_Settings.CraftSpeedMult, 0);
        }

        /*
         * Inter Patches Variables
         */

        static int currentItemID = 0; // item id set by ShowNotCraftableFromAssembly for GetFromProductionUI

        /*
         * ProductionInLoop Patch
         */

        [HarmonyPatch(typeof(FarmFactory), "AddFinshedProduct")]
        static class ProductionInLoop_Patch
        {
            static void Postfix(FarmFactory __instance, int id, int num)
            {
                if (!Enabled || !_Settings.ProductionInLoop)
                    return;

                Logger.Log($"move {num} of {id} from Production Output to Warehouse");

                __instance.RemoveFinshedProduct(id, num);
                __instance.MatList.Add(id, num);
            }
        }

        /*
         * PowerOnOutsideCtr Patch
         */

        [HarmonyPatch(typeof(FarmFactory_Outside_Ctr), "Start")]
        static class PowerOnOutsideCtr_Patch_Start
        {
            static void Postfix(FarmFactory_Outside_Ctr __instance, PlayerTargetMultiAction ___cmdTable)
            {
                if (!Enabled || !_Settings.PowerOnOutsideCtr)
                    return;

                ___cmdTable.RemoveAction(ActionType.ActionRevealTreasure);
                ___cmdTable.SetAction(ActionType.ActionRevealTreasure, TextMgr.GetStr(103439, -1), ActionTriggerMode.Normal);
            }
        }
        [HarmonyPatch(typeof(FarmFactory_Outside_Ctr), "CMDHandler")]
        static class PowerOnOutsideCtr_Patch_CMDHandler
        {
            static bool Prefix(FarmFactory_Outside_Ctr __instance, ActionType type)
            {
                if (!Enabled || !_Settings.PowerOnOutsideCtr)
                    return true;

                if (type == ActionType.ActionRevealTreasure)
                {
                    var playerItems = Module<Player>.Self.bag.GetAllItems(true);
                    var energyItems = new List<ItemObject>();
                    foreach (var itemIter in playerItems)
                        if (itemIter.ItemBase.Energy > 0)
                            energyItems.Add(itemIter);

                    UIStateMgr.Instance.ChangeStateByType(UIStateMgr.StateType.FarmFactory, false, new object[]
                    {
                        FarmFactoryState.UIType.PowerCenter, __instance.factoryId, energyItems
                    });

                    return false;
                }

                return true;
            }
        }

        /*
         * GetFromProductionUI Patch
         */

        [HarmonyPatch(typeof(FarmFactoryProductUICtr), "Init")]
        static class GetFromProductionUI_Patch_Init
        {
            static void Postfix(FarmFactoryProductUICtr __instance, GameObject ___addToListBtnSingle, GameObject ___addToListBtn)
            {
                if (Enabled && _Settings.GetFromProductionUI)
                {
                    ___addToListBtnSingle.GetComponentInChildren<TextMeshProUGUI>().text = TextMgr.GetStr(100509, -1);
                    ___addToListBtn.GetComponentInChildren<TextMeshProUGUI>().text = TextMgr.GetStr(100081, -1);
                }
            }
        }
        [HarmonyPatch(typeof(FarmFactoryProductUICtr), "AddCurItemToListSingle", new Type[] { })]
        static class GetFromProductionUI_Patch_AddCurItemToListSingle
        {
            static bool Prefix(FarmFactoryProductUICtr __instance, FarmFactory ___factory, CompoundItem ___curItem)
            {
                if (!Enabled || !_Settings.GetFromProductionUI)
                    return true;

                if (___curItem != null)
                    currentItemID = ___curItem.itemId;
                // when null continue and use current value of currentItemID (can be set to id of non-craftable item via ShowNotCraftableFromAssembly patch)

                int max = __instance.GetItemCount(currentItemID);
                Action<int> getItemsCallbackAction = delegate (int num)
                {
                    if (num < 0)
                        num = 0;
                    else if (num > max)
                        num = max;
                    Logger.Log($"get {num} (max = {max}) of {currentItemID} from factory");
                    Module<Player>.Self.bag.AddItem(currentItemID, num, true, AddItemMode.Default);
                    ___factory.RemoveMat(currentItemID, num);
                };
                string str = TextMgr.GetStr(100509, -1);

                NumberSelectWithItems uiGetItems = UIUtils.ShowNumberSelectWithMat(
                    currentItemID, 0, max, _Settings.GetFromProductionUIUseMax ? max : 1, str, getItemsCallbackAction, null, false, 0, string.Empty, null
                );
                uiGetItems.SetContent(str);

                return false;
            }
        }

        /*
         * FullWorktable Patch
         */

        [HarmonyPatch(typeof(CompoundItemData), "NotAutomable", MethodType.Getter)]
        static class FullWorktable_Patch
        {
            static void Postfix(ref bool __result, CompoundItemData __instance)
            {
                if (Enabled && _Settings.FullWorktable && __result && __instance.CompoundTime == 0)
                {
                    Logger.Log($"set {__instance.ID} as Automable");
                    __result = false;
                }
            }
        }
        [HarmonyPatch(typeof(CompoundTreeCtr), "Init")]
        static class FullWorktable_Patch2
        {
            static void Postfix(CompoundTreeCtr __instance, CompoundItem ___compound)
            {
                /*if (enabled && settings.FullWorktable && __result && __instance.CompoundTime == 0)
                {
                    Log($"set {__instance.ID} as Automable");
                    __result = false;
                }*/
                Logger.Log($"level on compound tree for {___compound.Id} is {__instance.level}");
                if (__instance.level > 4)
                {
                    Logger.Log(" - reduced to 4");
                    __instance.level = 4;
                }
            }
        }

        /*
         * ShowNotCraftableFromAssembly Patch
         */

        [HarmonyPatch(typeof(FarmFactoryProductUICtr), "ShowMissionPage")]
        static class ShowNotCraftableFromAssembly_Patch
        {
            static void Postfix(
                FarmFactoryProductUICtr __instance,
                Image ___creationTabImg, TextMeshProUGUI ___creationTabTips, int ___creationLevel, GameObject ___parent, GameObject ___pageItemPrefab,
                ItemListInfoUICtr ___infoCtr, TextMeshProUGUI ___extraText, TextMeshProUGUI ___curItemTimeText, TextMeshProUGUI ___curItemNameText, GameObject ___preview,
                GameObject ___changeItemBtn, GameObject ___addToListBtn, GameObject ___addToListBtnSingle, GameObject ___showCompoundBtn, GameObject ___cancelItemBtn
            )
            {
                if (!Enabled || !_Settings.ShowNotCraftableFromAssembly)
                    return;

                var creationStationNeeds = Module<CreationFactoryManager>.Self.GetMannualCreationNeedMaterial();
                foreach (var matIter in creationStationNeeds)
                {
                    if (Module<CompoundManager>.Self.GetitemInFactoryById(matIter.id) == null)
                    {
                        Logger.Log($"add {matIter.id} not craftable item from assembly station to mission page");

                        ___creationTabImg.sprite = Singleton<ResMgr>.Instance.LoadSyncByType<Sprite>(AssetType.UiSystem, ResPath.CreationTipsIcon[___creationLevel]);
                        ___creationTabImg.gameObject.SetActive(true);
                        ___creationTabTips.text = "x" + matIter.count.ToString();

                        GridIconWithNumAndName guiItem = GameUtils.AddChild(___parent, ___pageItemPrefab, false, true).GetComponentInChildren<GridIconWithNumAndName>();
                        guiItem.index = matIter.id;
                        guiItem.targetName.text = Module<ItemDataMgr>.Self.GetItemName(matIter.id);
                        guiItem.clickIcon.sprite = UIUtils.GetSpriteByPath(ItemObject.CreateItem(matIter.id, 1).ItemBase.IconPath);
                        guiItem.clickIcon.transform.parent.GetComponent<Image>().material.SetFloat("_Mask", 1f);
                        guiItem.clickIcon.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (__instance.GetItemCount(matIter.id) + ItemPackage.GetItemCount(matIter.id, true)).ToString();
                        guiItem.onSelectBg += delegate (int i)
                        {
                            currentItemID = i;
                            Logger.Log($"set currentItemID to {currentItemID} (for not craftable item)");

                            // list background
                            FieldInfo lastSelectField = __instance.GetType().GetField("lastSelect", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            GridIconWithNumAndName lastSelectVal = (GridIconWithNumAndName)lastSelectField.GetValue(__instance);
                            if (lastSelectVal != null && lastSelectVal != guiItem)
                                lastSelectVal.SelectBg(false);
                            lastSelectField.SetValue(__instance, guiItem);
                            __instance.GetType().GetField("curItem", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);
                            __instance.GetType().GetField("curItemWorkList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, false);

                            // item info and buttons
                            __instance.GetType().GetMethod("ShowInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { null });
                            ___curItemNameText.text = Module<ItemDataMgr>.Self.GetItemName(currentItemID); ;
                            ___addToListBtnSingle.SetActive(_Settings.GetFromProductionUI);

                            // item preview
                            try
                            {
                                ((ItemPreviewCam)__instance.GetType().GetField("prev", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).SetItem(currentItemID);
                                ___preview.SetActive(true);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("preview error: " + ex.ToString());
                            }
                        };
                    }
                }
            }
        }

        /*
         * UseArtisanSkill Patch
         */

        [HarmonyPatch(typeof(FarmFactory), "ReCaculeteItemCapacityDepth")]
        static class FarmFactory_ReCaculeteItemCapacityDepth_Patch
        {
            static void Postfix(int itemId, List<IdCount> mat, int num)
            {
                if (!Enabled || !_Settings.UseArtisanSkill)
                    return;

                float discount = Module<FeatureModule>.Self.ModifyFloat(FeatureType.WorkbenchMaterialCut, new object[] { 0f });
                Logger.Log($"crafting discount is {discount}, itemId is {itemId}");

                if (discount > 0)
                {
                    CompoundItem item = Module<CompoundManager>.Self.GetitemInFactoryById(itemId);
                    if (item != null && item.CompoundTime == 0)
                    {
                        foreach (var matIter in mat)
                        {
                            int newCount = (int)Math.Round(matIter.count * (1 - discount));
                            if (newCount < 1)
                                newCount = 1;
                            Logger.Log($"applay {discount * 100}% discount to {matIter.id} used for {num} x {item.Name}, old count = {matIter.count}, new count = newCount");
                            matIter.count = newCount;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FarmFactory), "GetDashBoardSpeed")]
        static class FarmFactory_GetDashBoardSpeed_Patch
        {
            static void Postfix(ref float __result)
            {
                if (!Enabled) return;

                __result *= _Settings.CraftSpeedMult;
            }
        }
    }
}
