using GoBangGameNs;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace GoBangDivineMove
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

        [HarmonyPatch(typeof(MainLoop), "Update")]
        public class MainLoop_Update_Patch
        {
            public static int lastStepCount = -1;
            private static int lockedUIX = 7;
            private static int lockedUIY = 7;

            public static void Postfix(MainLoop __instance)
            {
                if (!Main.Enabled) return;

                var traverse = Traverse.Create(__instance);

                GoBang_GameStatus status = traverse.Field("_Status").GetValue<GoBang_GameStatus>();
                int cntStep = traverse.Field("cnt_step").GetValue<int>();

                if (status == GoBang_GameStatus.BlackTurn)
                {
                    Board chessBoardUI = traverse.Field("ChessBoard").GetValue<Board>();
                    BoardModel boardModel = traverse.Field("_ChessBoardModel").GetValue<BoardModel>();
                    object computerAI = traverse.Field("Computer").GetValue();

                    if (chessBoardUI == null || boardModel == null || computerAI == null) return;
                    
                    if (cntStep != lastStepCount)
                    {
                        lockedUIX = lockedUIY = 7;

                        if (cntStep > 0)
                        {
                            ChessType[,] invertedBoard = new ChessType[15, 15];
                            ChessType[,] trueBoard = new ChessType[15, 15];
                            for (int x = 0; x < 15; x++)
                            {
                                for (int y = 0; y < 15; y++)
                                {
                                    ChessType realType = boardModel.getType(x, 14 - y); // 14-y修正鏡像

                                    trueBoard[x, y] = realType;

                                    if (realType == ChessType.Black) invertedBoard[x, y] = ChessType.White;
                                    else if (realType == ChessType.White) invertedBoard[x, y] = ChessType.Black;
                                    else invertedBoard[x, y] = ChessType.None;
                                }
                            }

                            var aiTraverse = Traverse.Create(computerAI);
                            aiTraverse.Field("chessBoard").SetValue(invertedBoard);

                            object hintPosObj = null;
                            try
                            {
                                var getPosMethod = computerAI.GetType().GetMethod("getPos", BindingFlags.Public | BindingFlags.Instance);
                                if (getPosMethod != null)
                                {
                                    hintPosObj = getPosMethod.Invoke(computerAI, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Main.Logger.Error($"呼叫原廠 getPos 失敗: {ex.Message}");
                            }

                            aiTraverse.Field("chessBoard").SetValue(trueBoard);

                            if (hintPosObj != null)
                            {
                                int rawX = 0;
                                int rawY = 0;

                                var fX = hintPosObj.GetType().GetField("pX", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                                var fY = hintPosObj.GetType().GetField("pY", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                                if (fX != null && fY != null)
                                {
                                    rawX = (int)fX.GetValue(hintPosObj);
                                    rawY = (int)fY.GetValue(hintPosObj);
                                }
                                else
                                {
                                    var pX = hintPosObj.GetType().GetProperty("pX", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                                    var pY = hintPosObj.GetType().GetProperty("pY", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                                    if (pX != null && pY != null)
                                    {
                                        rawX = (int)pX.GetValue(hintPosObj, null);
                                        rawY = (int)pY.GetValue(hintPosObj, null);
                                    }
                                }

                                lockedUIX = rawX;
                                lockedUIY = 14 - rawY;
                            }
                        }
                        Main.Logger.Log($"[GoBang Divine Move] 輪到玩家回合！神之一手預測座標為: X = {lockedUIX}, Y = {lockedUIY}");
                        lastStepCount = cntStep;
                    }
                    HighlightSquare(chessBoardUI, lockedUIX, lockedUIY);
                }
                else if (status == GoBang_GameStatus.Pending && lastStepCount != -1)
                {
                    lastStepCount = -1;
                }
            }

            private static void HighlightSquare(Board chessBoardUI, int x, int y)
            {
                Cross bestCross = chessBoardUI.getCross(x, y);
                if (bestCross != null)
                {
                    var buttonComponent = bestCross.GetComponent<Button>();
                    if (buttonComponent != null)
                    {
                        Main.Logger.Log($"[GoBang Divine Move] 成功找到 Button，強制執行 Select()");
                        buttonComponent.Select();
                    }
                }
            }
        }
    }
}
