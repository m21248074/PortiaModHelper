/*
Set of tweaks for Factory:
 * automatically move production from production output storage to main factory warehouse
 * replace open Product Warehouse action by open Central Power Supply on external factory control panel
 * add get button (instead of single query craft) to factory production UI
 * enable convert hardwood in wood in factory
 * show not craftable materials needed for Assembly Station on "mission" tab in Auto Worktable
 * use Artisan Skill on Auto Worktable (works per individual item in production query - as discount in shops)
Each of above feature can be independently switch on/off via mod settings.

Feel free to modify, redistribute, etc.

Author: rrpbercik
Version: 0.1.3 (2022-06-08)
*/

using Harmony12;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

using Pathea;
using Pathea.ModuleNs;
using Pathea.FarmFactoryNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.CompoundSystem;
using Pathea.CreationFactory;
using TMPro;

using Pathea.FeatureNs;


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

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
	
    public class Main
    {
        public static Settings settings { get; private set; }

        public static bool enabled;

        static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ProductionInLoop = GUILayout.Toggle(settings.ProductionInLoop, "Automatically move factory production to Material Warehouse", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.PowerOnOutsideCtr = GUILayout.Toggle(settings.PowerOnOutsideCtr, "Replace open Product Warehouse action by open Central Power Supply on external factory control panel.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.GetFromProductionUI = GUILayout.Toggle(settings.GetFromProductionUI, "Add get button (instead of single query craft) to factory production UI. Used to get items from factory storage to plaer inventory.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.GetFromProductionUIUseMax = GUILayout.Toggle(settings.GetFromProductionUIUseMax, "Set items count in \"Get\" dialog to max", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.FullWorktable = GUILayout.Toggle(settings.FullWorktable, "Enable convert hardwood in wood in factory (exactly allow to craft \"NotAutomable\" but \"Instant\" items on Auto Worktable).", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.ShowNotCraftableFromAssembly = GUILayout.Toggle(settings.ShowNotCraftableFromAssembly, "Show not craftable materials needed for Assembly Station on Auto Worktable. Useful for \"get\" option.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.UseArtisanSkill = GUILayout.Toggle(settings.UseArtisanSkill, "Use Artisan Skill on Auto Worktable. Discount will be apply per individual item in query and rounded (not per full query, as on standard Worktable).", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.Debug = GUILayout.Toggle(settings.Debug, "Write debug info to log.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
        }

        static void Log(string info)
        {
            if (settings.Debug)
                Debug.Log("BetterFactory: " + info);
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
                if (!enabled || !settings.ProductionInLoop)
                    return;

                Log($"move {num} of {id} from Production Output to Warehouse");

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
                if (!enabled || !settings.PowerOnOutsideCtr)
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
                if (!enabled || !settings.PowerOnOutsideCtr)
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
                if (enabled && settings.GetFromProductionUI)
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
                if (!enabled || !settings.GetFromProductionUI)
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
                    Log($"get {num} (max = {max}) of {currentItemID} from factory");
                    Module<Player>.Self.bag.AddItem(currentItemID, num, true, AddItemMode.Default);
                    ___factory.RemoveMat(currentItemID, num);
                };
                string str = TextMgr.GetStr(100509, -1);
                
                NumberSelectWithItems uiGetItems = UIUtils.ShowNumberSelectWithMat(
                    currentItemID, 0, max, settings.GetFromProductionUIUseMax ? max : 1, str, getItemsCallbackAction, null, false, 0, string.Empty, null
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
                if (enabled && settings.FullWorktable && __result && __instance.CompoundTime == 0)
                {
                    Log($"set {__instance.ID} as Automable");
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
				Log($"level on compound tree for {___compound.Id} is {__instance.level}");
                if (__instance.level > 4)
                {
                    Log(" - reduced to 4");
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
                if (!enabled || !settings.ShowNotCraftableFromAssembly)
                    return;

                var creationStationNeeds = Module<CreationFactoryManager>.Self.GetMannualCreationNeedMaterial();
                foreach (var matIter in creationStationNeeds)
                {
                    if (Module<CompoundManager>.Self.GetitemInFactoryById(matIter.id) == null)
                    {
                        Log($"add {matIter.id} not craftable item from assembly station to mission page");

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
                            Log($"set currentItemID to {currentItemID} (for not craftable item)");

                            // list background
                            FieldInfo lastSelectField = __instance.GetType().GetField("lastSelect", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            GridIconWithNumAndName lastSelectVal = (GridIconWithNumAndName)lastSelectField.GetValue(__instance);
                            if (lastSelectVal != null && lastSelectVal != guiItem)
                                lastSelectVal.SelectBg(false);
                            lastSelectField.SetValue(__instance, guiItem);
                            __instance.GetType().GetField("curItem", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);
                            __instance.GetType().GetField("curItemWorkList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, false);

                            // item info and buttons
                            __instance.GetType().GetMethod("ShowInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] {null});
                            ___curItemNameText.text = Module<ItemDataMgr>.Self.GetItemName(currentItemID); ;
                            ___addToListBtnSingle.SetActive(settings.GetFromProductionUI);

                            // item preview
                            try
                            {
                                ((ItemPreviewCam)__instance.GetType().GetField("prev", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).SetItem(currentItemID);
                                ___preview.SetActive(true);
                            }
                            catch (Exception ex)
                            {
                                Log("preview error: " + ex.ToString());
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
                if (!enabled || !settings.UseArtisanSkill)
                    return;

                float discount = Module<FeatureModule>.Self.ModifyFloat(FeatureType.WorkbenchMaterialCut, new object[] { 0f });
                Log($"crafting discount is {discount}, itemId is {itemId}");

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
                            Log($"applay {discount * 100}% discount to {matIter.id} used for {num} x {item.Name}, old count = {matIter.count}, new count = newCount");
                            matIter.count = newCount;
                        }
                    }
                }
            }
        }
    }
}
