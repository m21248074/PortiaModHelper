using Pathea;
using Pathea.ModuleNs;
using RelicManager.Core;
using RelicManager.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace RelicManager.UI
{
	// Token: 0x02000005 RID: 5
	internal class RelicManagerGui : MonoBehaviour
	{
		// Token: 0x06000019 RID: 25 RVA: 0x00003228 File Offset: 0x00001428
		public void Awake()
		{
			int num = Screen.width / 2 - this._width / 2;
			int num2 = Screen.height / 2 - this._height / 2;
			this._windowRect = new Rect((float)num, (float)num2, (float)this._width, (float)this._height);
			this._headingsFontStyle = new GUIStyle
			{
				fontSize = 20,
				fontStyle = 0,
				wordWrap = true,
				alignment = TextAnchor.MiddleCenter
			};
			this._contentFontStyle = new GUIStyle
			{
				fontSize = 16
			};
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000032B0 File Offset: 0x000014B0
		private void OnEnable()
		{
			if (this._loading || !Controller.Instance.ConfirmEnabled)
			{
				return;
			}
			this._loading = true;
			new Thread(delegate()
			{
				foreach (Relic relic in Relics.RelicsList)
				{
					relic.GetPiecesOwnedCounts();
				}
				this._discsOwned = ItemRetriever.GetOwnedByID(Relics.DataDiscID);
				Controller.Instance.PauseGame();
				this._loading = false;
			})
			{
				Name = "LoadRelicManager",
				IsBackground = true
			}.Start();
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00003301 File Offset: 0x00001501
		private void OnDisable()
		{
			Controller.Instance.ResumeGame();
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00003310 File Offset: 0x00001510
		private void OnGUI()
		{
			if (!Controller.Instance.ConfirmEnabled)
			{
				return;
			}
			if (!this._loading)
			{
				this._windowRect = GUI.Window(1, this._windowRect, new GUI.WindowFunction(this.RenderWindow), "古物管理器");
				return;
			}
			GUI.Button(new Rect((float)(Screen.width / 2), (float)(Screen.height / 2), 100f, 25f), "Loading...");
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003380 File Offset: 0x00001580
		private void RenderWindow(int windowId)
		{
			this.textColour = "white";
			GUILayout.BeginHorizontal(new GUILayoutOption[]
			{
				GUILayout.Height(35f)
			});
			this._headingsFontStyle.alignment = TextAnchor.MiddleLeft;
			GUILayout.Label("<color=" + this.textColour + ">名稱</color>", this._headingsFontStyle, new GUILayoutOption[]
			{
				GUILayout.Width((float)this.nameColumnWidth)
			});
			this._headingsFontStyle.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label("<color=" + this.textColour + ">博物館</color>", this._headingsFontStyle, new GUILayoutOption[]
			{
				GUILayout.Width((float)this.inMuseumColumnWidth)
			});
			GUILayout.Label("<color=" + this.textColour + ">碎片</color>", this._headingsFontStyle, new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(true)
			});
			GUILayout.Space((float)this.buttonColumn);
			GUILayout.EndHorizontal();
			this._scrollPosition = GUILayout.BeginScrollView(this._scrollPosition, new GUILayoutOption[0]);
			foreach (Relic relic in Relics.RelicsList)
			{
				if (relic.PiecesOwnedCounts.All((int num) => num > 0))
				{
					this.textColour = "lime";
				}
				else if (relic.PiecesOwnedCounts.All((int num) => num == 0))
				{
					this.textColour = "red";
				}
				else
				{
					this.textColour = "white";
				}
				GUILayout.BeginHorizontal(new GUILayoutOption[]
				{
					GUILayout.Height((float)this.rowHeight)
				});
				this._contentFontStyle.alignment = TextAnchor.MiddleLeft;
				GUILayout.Label(string.Concat(new string[]
				{
					"<color=",
					this.textColour,
					">",
					relic.Name,
					"</color>"
				}), this._contentFontStyle, new GUILayoutOption[]
				{
					GUILayout.Width((float)this.nameColumnWidth)
				});
				this._contentFontStyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label(string.Concat(new string[]
				{
					"<color=",
					this.textColour,
					">",
					relic.InMuseum ? "有" : "無",
					"</color>"
				}), this._contentFontStyle, new GUILayoutOption[]
				{
					GUILayout.Width((float)this.inMuseumColumnWidth)
				});
				GUILayout.FlexibleSpace();
				for (int i = 0; i < 5; i++)
				{
					if (i < relic.Pieces)
					{
						GUILayout.Label(string.Format("<color={0}>{1}</color>", this.textColour, relic.PiecesOwnedCounts[i]), this._contentFontStyle, new GUILayoutOption[]
						{
							GUILayout.Width((float)this.pieceColumnWidth)
						});
						GUILayout.FlexibleSpace();
					}
					else
					{
						this._contentFontStyle.fontSize = 14;
						GUILayout.Label("<color=" + this.textColour + ">X</color>", this._contentFontStyle, new GUILayoutOption[]
						{
							GUILayout.Width((float)this.pieceColumnWidth)
						});
						this._contentFontStyle.fontSize = 16;
						GUILayout.FlexibleSpace();
					}
				}
				if (GUILayout.Button("<color=" + this.textColour + ">取出</color>", new GUILayoutOption[]
				{
					GUILayout.Width((float)this.buttonColumn)
				}))
				{
					relic.PullRelicPieces();
					if (this._pullDiscs && this._discsOwned > 0)
					{
						relic.PullDataDiscs();
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();

            GUILayout.Box("", new GUILayoutOption[] { GUILayout.Height(2f), GUILayout.ExpandWidth(true) });

            // 繪製四組控制按鈕的橫列 (高度固定 30f)
            GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Height(30f) });

            // ------------------ 【區塊 A：拿】 ------------------
            GUILayout.Label("<color=white>【拿】</color>", this._contentFontStyle, GUILayout.Width(40f));

            // A1 按鈕 (寬 150f)
            string btnA1Label = (this._selectedA1RelicIdx != -1) ? Relics.RelicsList[this._selectedA1RelicIdx].Name : "選擇古物...";
            if (GUILayout.Button(btnA1Label + (this._showComboA1 ? " ▲" : " ▼"), GUILayout.Width(150f)))
            {
                this._showComboA1 = !this._showComboA1;
                this._showComboA2 = this._showComboB1 = this._showComboB2 = false; // 點它就關閉其他
            }

            GUILayout.Label("<color=white>碎片:</color>", this._contentFontStyle, GUILayout.Width(40f));

            // A2 按鈕 (寬 80f) —— 沒選 A1 前不能點
            string btnA2Label = (this._selectedA2PieceIdx != -1) ? $"#{this._selectedA2PieceIdx + 1}" : "選擇...";
            GUI.enabled = (this._selectedA1RelicIdx != -1);
            if (GUILayout.Button(btnA2Label + (this._showComboA2 ? " ▲" : " ▼"), GUILayout.Width(80f)))
            {
                this._showComboA2 = !this._showComboA2;
                this._showComboA1 = this._showComboB1 = this._showComboB2 = false;
            }
            GUI.enabled = true;

            // 中間箭頭過渡
            GUILayout.Label("<color=yellow> ➔ </color>", this._contentFontStyle, GUILayout.Width(25f));

            // ------------------ 【區塊 B：換】 ------------------
            GUILayout.Label("<color=lime>【換】</color>", this._contentFontStyle, GUILayout.Width(40f));

            // B1 按鈕 (寬 150f)
            string btnB1Label = (this._selectedB1RelicIdx != -1) ? Relics.RelicsList[this._selectedB1RelicIdx].Name : "選擇古物...";
            if (GUILayout.Button(btnB1Label + (this._showComboB1 ? " ▲" : " ▼"), GUILayout.Width(150f)))
            {
                this._showComboB1 = !this._showComboB1;
                this._showComboA1 = this._showComboA2 = this._showComboB2 = false;
            }

            GUILayout.Label("<color=lime>碎片:</color>", this._contentFontStyle, GUILayout.Width(40f));

            // B2 按鈕 (寬 80f) —— 沒選 B1 前不能點
            string btnB2Label = (this._selectedB2PieceIdx != -1) ? $"#{this._selectedB2PieceIdx + 1}" : "選擇...";
            GUI.enabled = (this._selectedB1RelicIdx != -1);
            if (GUILayout.Button(btnB2Label + (this._showComboB2 ? " ▲" : " ▼"), GUILayout.Width(80f)))
            {
                this._showComboB2 = !this._showComboB2;
                this._showComboA1 = this._showComboA2 = this._showComboB1 = false;
            }
            GUI.enabled = true;

            GUILayout.Space(15f);

            // ------------------ 【交換動作按鈕】 ------------------
            // 安全防呆：4個都要選，且不能自己換自己（同古物且同碎片）
            bool hasValidSelection = (this._selectedA1RelicIdx != -1 && this._selectedA2PieceIdx != -1 && this._selectedB1RelicIdx != -1 && this._selectedB2PieceIdx != -1);
            bool isNotSamePiece = !(this._selectedA1RelicIdx == this._selectedB1RelicIdx && this._selectedA2PieceIdx == this._selectedB2PieceIdx);

            GUI.enabled = (hasValidSelection && isNotSamePiece);
            if (GUILayout.Button("<color=lime><b>交換</b></color>", GUILayout.Width(65f)))
            {
                Relic sourceRelic = Relics.RelicsList[this._selectedA1RelicIdx];
                Relic targetRelic = Relics.RelicsList[this._selectedB1RelicIdx];

				bool isSuccess = Relic.ExchangePieces(sourceRelic, this._selectedA2PieceIdx, targetRelic, this._selectedB2PieceIdx);

                if (isSuccess)
                {
                    // 換完後重設「碎片選取」，避免連點誤觸
                    this._selectedA2PieceIdx = -1;
                    this._selectedB2PieceIdx = -1;
                }
                else
                {
                    Main.Logger.Log("【交換失敗】");
                }
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // 3. -------------------------------------------------------------
            // 繪製原地展開清單 (依據當前打開哪一個選單，原地向下推開等寬區塊)
            // -------------------------------------------------------------

            // --- 展開 A1：來源古物選單 ---
            if (this._showComboA1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(45f); // 對齊 A1 按鈕開頭 (【拿】寬度 40f + 少量微調)

                List<string> availableNamesA1 = new List<string>();
                List<int> availableIndicesA1 = new List<int>();
                for (int i = 0; i < Relics.RelicsList.Count; i++)
                {
                    if (Relics.RelicsList[i].PiecesOwnedCounts.Any(num => num > 0)) // 篩選：手頭至少有一個碎片
                    {
                        availableNamesA1.Add(Relics.RelicsList[i].Name);
                        availableIndicesA1.Add(i);
                    }
                }

                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150f)); // 等寬 150f
                this._scrollA1 = GUILayout.BeginScrollView(this._scrollA1, GUILayout.Height(100f));
                int gridResult = GUILayout.SelectionGrid(-1, availableNamesA1.ToArray(), 1);
                if (gridResult != -1)
                {
                    this._selectedA1RelicIdx = availableIndicesA1[gridResult];
                    this._selectedA2PieceIdx = -1; // 切換古物就清空碎片
                    this._showComboA1 = false;
                }
                if (availableNamesA1.Count == 0) GUILayout.Label("<color=red>無可用古物</color>");
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            // --- 展開 A2：來源碎片選單 ---
            if (this._showComboA2 && this._selectedA1RelicIdx != -1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(235f); // 對齊 A2 按鈕開頭 (40 + 150 + 40 + 微調)

                Relic rA = Relics.RelicsList[this._selectedA1RelicIdx];
                List<string> availablePiecesA2 = new List<string>();
                List<int> availableIndicesA2 = new List<int>();
                for (int i = 0; i < rA.Pieces; i++)
                {
                    if (rA.PiecesOwnedCounts[i] > 0) // 篩選：自己持有量 > 0 的碎片
                    {
                        availablePiecesA2.Add($"碎片 {i + 1} ({rA.PiecesOwnedCounts[i]}個)");
                        availableIndicesA2.Add(i);
                    }
                }

                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(80f)); // 等寬 80f
                this._scrollA2 = GUILayout.BeginScrollView(this._scrollA2, GUILayout.Height(100f));
                int gridResult = GUILayout.SelectionGrid(-1, availablePiecesA2.ToArray(), 1);
                if (gridResult != -1)
                {
                    this._selectedA2PieceIdx = availableIndicesA2[gridResult];
                    this._showComboA2 = false;
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            // --- 展開 B1：目標古物選單 ---
            if (this._showComboB1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(340f); // 對齊 B1 按鈕開頭

                List<string> allNames = Relics.RelicsList.Select(r => r.Name).ToList();

                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150f)); // 等寬 150f
                this._scrollB1 = GUILayout.BeginScrollView(this._scrollB1, GUILayout.Height(100f));
                int gridResult = GUILayout.SelectionGrid(-1, allNames.ToArray(), 1);
                if (gridResult != -1)
                {
                    this._selectedB1RelicIdx = gridResult; // 因為全列，網格索引直接等於古物清單索引
                    this._selectedB2PieceIdx = -1; // 切換古物就清空碎片
                    this._showComboB1 = false;
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            // --- 展開 B2：目標碎片選單 ---
            if (this._showComboB2 && this._selectedB1RelicIdx != -1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(530f); // 對齊 B2 按鈕開頭

                Relic rB = Relics.RelicsList[this._selectedB1RelicIdx];
                List<string> allPiecesB2 = new List<string>();
                for (int i = 0; i < rB.Pieces; i++)
                {
                    allPiecesB2.Add($"碎片 {i + 1}"); // 目標古物的所有碎片全列
                }

                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(80f)); // 等寬 80f
                this._scrollB2 = GUILayout.BeginScrollView(this._scrollB2, GUILayout.Height(100f));
                int gridResult = GUILayout.SelectionGrid(-1, allPiecesB2.ToArray(), 1);
                if (gridResult != -1)
                {
                    this._selectedB2PieceIdx = gridResult;
                    this._showComboB2 = false;
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Box("", new GUILayoutOption[] { GUILayout.Height(1f), GUILayout.ExpandWidth(true) });

            GUILayout.BeginVertical(GUI.skin.box); // 用一個獨立的 Box 把賭場框起來

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Label("<color=yellow><b>🎰 帕斯亞皇家娛樂場 🎰</b></color> <color=white>| 規則：僅會抽出你「正在收集且未進博物館」的古物碎片！</color>", this._contentFontStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Height(35f) });
            GUILayout.Label("<color=gray>選擇你的籌碼下注：</color>", this._contentFontStyle, GUILayout.Width(130f));

            // 【按鈕 1：小賭怡情】
            if (GUILayout.Button("<color=lime><b>小賭怡情 (250 金幣)</b></color>", GUILayout.Width(140f), GUILayout.Height(26f)))
            {
                Relic.GambleForPiece(1);
            }
            GUILayout.Space(10f);

            // 【按鈕 2：大賭興家】
            if (GUILayout.Button("<color=yellow><b>豪賭一波 (500 金幣)</b></color>", GUILayout.Width(140f), GUILayout.Height(26f)))
            {
                Relic.GambleForPiece(2);
            }
            GUILayout.Space(10f);

            // 【按鈕 3：天台見（孤注一擲）】
            if (GUILayout.Button("<color=red><b>🔥 孤注一擲 (1000 金幣)</b></color>", GUILayout.Width(160f), GUILayout.Height(26f)))
            {
                Relic.GambleForPiece(3);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			GUILayout.Label(string.Format("<color=white>擁有的資料光碟: {0}</color>", this._discsOwned), new GUILayoutOption[0]);
			GUILayout.Space(10f);
			this._pullDiscs = GUILayout.Toggle(this._pullDiscs, "<color=white>取出資料光碟</color>", new GUILayoutOption[0]);
			GUILayout.Space(7f);
			GUILayout.EndHorizontal();
		}

		// Token: 0x04000012 RID: 18
		private readonly int _width = 800;

		// Token: 0x04000013 RID: 19
		private readonly int _height = 500;

		// Token: 0x04000014 RID: 20
		private Rect _windowRect;

		// Token: 0x04000015 RID: 21
		private GUIStyle _headingsFontStyle;

		// Token: 0x04000016 RID: 22
		private GUIStyle _contentFontStyle;

		// Token: 0x04000017 RID: 23
		private Vector2 _scrollPosition;

		// Token: 0x04000018 RID: 24
		private bool _loading;

		// Token: 0x04000019 RID: 25
		private bool _pullDiscs = true;

		// Token: 0x0400001A RID: 26
		private int _discsOwned;

		// Token: 0x0400001B RID: 27
		private int nameColumnWidth = 200;

		// Token: 0x0400001C RID: 28
		private int inMuseumColumnWidth = 80;

		// Token: 0x0400001D RID: 29
		private int pieceColumnWidth = 30;

		// Token: 0x0400001E RID: 30
		private int buttonColumn = 60;

		// Token: 0x0400001F RID: 31
		private int rowHeight = 20;

		// Token: 0x04000020 RID: 32
		private string textColour = "white";

        private bool _showComboA1 = false;
        private bool _showComboA2 = false;
        private bool _showComboB1 = false;
        private bool _showComboB2 = false;
        private int _selectedA1RelicIdx = -1;
		private int _selectedA2PieceIdx = -1;
        private int _selectedB1RelicIdx = -1;
        private int _selectedB2PieceIdx = -1;
        private Vector2 _scrollA1 = Vector2.zero;
        private Vector2 _scrollA2 = Vector2.zero;
        private Vector2 _scrollB1 = Vector2.zero;
        private Vector2 _scrollB2 = Vector2.zero;
    }
}
