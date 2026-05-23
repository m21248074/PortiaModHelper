using System;
using System.Linq;
using System.Threading;
using RelicManager.Core;
using RelicManager.Modules;
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
				alignment = TextAnchor.MiddleCenter,
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
				this._windowRect = GUI.Window(1, this._windowRect, new GUI.WindowFunction(this.RenderWindow), "Relic Manager");
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
			GUILayout.Label("<color=" + this.textColour + ">Name</color>", this._headingsFontStyle, new GUILayoutOption[]
			{
				GUILayout.Width((float)this.nameColumnWidth)
			});
			this._headingsFontStyle.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label("<color=" + this.textColour + ">Museum</color>", this._headingsFontStyle, new GUILayoutOption[]
			{
				GUILayout.Width((float)this.inMuseumColumnWidth)
			});
			GUILayout.Label("<color=" + this.textColour + ">Pieces</color>", this._headingsFontStyle, new GUILayoutOption[]
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
					relic.InMuseum ? "Yes" : "No",
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
				if (GUILayout.Button("<color=" + this.textColour + ">Pull</color>", new GUILayoutOption[]
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
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			GUILayout.Label(string.Format("<color=white>Data discs owned: {0}</color>", this._discsOwned), new GUILayoutOption[0]);
			GUILayout.Space(10f);
			this._pullDiscs = GUILayout.Toggle(this._pullDiscs, "<color=white>Pull data discs</color>", new GUILayoutOption[0]);
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
	}
}
