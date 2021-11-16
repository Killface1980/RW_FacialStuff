namespace PawnPlus.UI
{
    using System.Collections.Generic;

    using PawnPlus.Defs;
    using PawnPlus.Parts;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public partial class DialogEditAppearance : Window
	{
		private static Color kMenuSectionBGFillColor = new ColorInt(42, 43, 44).ToColor;
		private static Color KMenuSectionBGBorderColor = new ColorInt(135, 135, 135).ToColor;
		private const float kMargin = 7f;
		private const float kPanelMargin = 4;

		public override Vector2 InitialSize => new Vector2(750f, 750);

		private Pawn _pawn;
		private CompFace _compPawnPlus;
		private int _categoryTabIndex;
		private PartRendererBase _curPartRenderer;
		private Vector2 _scrollPos = Vector2.zero;
		private List<CategoryWorker> _categoryWorkers = new List<CategoryWorker>();
				
		public DialogEditAppearance(Pawn pawn)
		{
			this.doCloseX = true;
			_pawn = pawn;
			_categoryWorkers.Add(new CategoryWorkerHair());
			if(!PartDef._allParts.TryGetValue(pawn.RaceProps.body, out Dictionary<PartCategoryDef, List<PartDef>> partDefs))
			{
				return;
			}

			foreach(KeyValuePair<PartCategoryDef, List<PartDef>> pair in partDefs)
			{
				PartCategoryDef partCategoryDef = pair.Key;
				List<PartDef> partsInCategory = pair.Value;
				_categoryWorkers.Add(new CategoryWorkerPartDef(partCategoryDef, partsInCategory));
			}

			_compPawnPlus = _pawn.GetComp<CompFace>();
		}

		public override void DoWindowContents(Rect inRect)
		{
			if(_compPawnPlus == null)
			{
				Widgets.Label(inRect, "Pawn does not have PawnPlus component enabled");
				return;
			}

			float titleHeight = 30f;
			Rect titleRect = inRect.TopPartPixels(titleHeight);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(titleRect, "Edit Appearance for " + _pawn.LabelShort);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;

			List<TabRecord> categoryTabs = new List<TabRecord>();
			int tabIndex = 0;
			foreach(CategoryWorker categoryWorker in _categoryWorkers)
			{
				int tabIndexCopy = tabIndex;
				categoryTabs.Add(new TabRecord(categoryWorker.CategoryTabLabel, delegate { _categoryTabIndex = tabIndexCopy; }, _categoryTabIndex == tabIndex));
				++tabIndex;
			}

			CategoryWorker curCategoryWorker = _categoryWorkers[_categoryTabIndex];

			float contentYLoc = titleHeight + TabDrawer.TabHeight + kMargin;
			Rect bottomRect = inRect.BottomPartPixels(50);
			Rect contentRect = new Rect(0f, contentYLoc, inRect.width, inRect.height - contentYLoc - bottomRect.height);
			TabDrawer.DrawTabs(contentRect, categoryTabs);
			GUI.BeginGroup(contentRect);
			{
				Rect localContentRect = contentRect.AtZero();
				float leftRectWidth = 240f;
				Rect leftRect = localContentRect.LeftPartPixels(leftRectWidth);
				Rect rightRect = localContentRect.RightPartPixels(localContentRect.width - leftRectWidth);
				leftRect = leftRect.ContractedBy(kMargin);
				rightRect = rightRect.ContractedBy(kMargin);
				
				float portraitRectHeight = 200f;
				Rect portraitRect = leftRect.TopPartPixels(portraitRectHeight);
				GUI.BeginGroup(portraitRect);
				{
					Rect localPortraitRect = portraitRect.AtZero();
					GUI.DrawTexture(localPortraitRect, ColonistBar.BGTex);
					GUI.DrawTexture(
						localPortraitRect,
						PortraitsCache.Get(
							_pawn, 
							new Vector2(
								localPortraitRect.width, 
								localPortraitRect.height), 
							Rot4.South,
							new Vector3(0f, 0f, 0.35f), 
							1.25f));
					GUI.color = Color.white;
				}

				GUI.EndGroup();

				Rect slidersRect = new Rect(
					leftRect.x, 
					portraitRect.y + portraitRectHeight + kMargin, 
					leftRect.width, 
					leftRect.height - portraitRectHeight - kMargin);
				GUI.BeginGroup(slidersRect);
				{
					Rect localSlidersRect = slidersRect.AtZero();
					Widgets.DrawBoxSolid(localSlidersRect, kMenuSectionBGFillColor);
					Widgets.DrawBox(localSlidersRect);
				}

				GUI.EndGroup();

				GUI.BeginGroup(rightRect);
				{
					Rect localRightRect = rightRect.AtZero();
					Rect genderTabRect = new Rect(
						localRightRect.x, 
						localRightRect.y += TabDrawer.TabHeight,
						localRightRect.width,
						localRightRect.height - TabDrawer.TabHeight);
					TabDrawer.DrawTabs(genderTabRect, curCategoryWorker.GetGenderTabs());
					
					float tabHeightGap = 2f;
					Rect tagsTabRect = new Rect(
						genderTabRect.x,
						genderTabRect.y += TabDrawer.TabHeight + tabHeightGap,
						genderTabRect.width,
						genderTabRect.height - TabDrawer.TabHeight - tabHeightGap);
					TabDrawer.DrawTabs(tagsTabRect, curCategoryWorker.GetTagTabs());
					Rect scrollViewRect = tagsTabRect.ContractedBy(2f);
					Rect scrollViewContentRect = new Rect(
                        0,
                        0,

                        // Subtract 2 from width to hide horizontal scroll bar
                        scrollViewRect.width - GUI.skin.verticalScrollbar.fixedWidth - 2f,
                        1000);
					Widgets.BeginScrollView(
						scrollViewRect, 
						ref curCategoryWorker.scrollPos,
						scrollViewContentRect);
					{
						Widgets.DrawBoxSolid(scrollViewContentRect, kMenuSectionBGFillColor);
						GUI.BeginClip(scrollViewContentRect);
						int count = 10;
						float panelWidth = scrollViewContentRect.width / 2;
						float panelHeight = 100f;
						for(int i = 0; i < count; ++i)
						{
							int row = i / 2;
							int column = i % 2;
							Rect panelRect = new Rect(panelWidth * column, panelHeight * row, panelWidth, panelHeight);
							panelRect = panelRect.ContractedBy(kPanelMargin);
							switch(i % 4)
							{ 
								case 0:
									Widgets.DrawAltRect(panelRect);
									break;

								case 1:
									Widgets.DrawHighlightSelected(panelRect);
									break;

								case 2:
									Widgets.DrawHighlight(panelRect);
									break;

								case 3:
									Widgets.DrawHighlightIfMouseover(panelRect);
									break;
							}
						}

						GUI.EndClip();
					}

					Widgets.EndScrollView();
				}

				GUI.EndGroup();
			}

			GUI.EndGroup();

			bottomRect = bottomRect.ContractedBy(kMargin);
			GUI.BeginGroup(bottomRect);
			{
				Rect localBottomRect = bottomRect.AtZero();
				float buttonWidth = 120f;
				Text.Font = GameFont.Small;
				Rect btnApplyRect = new Rect(localBottomRect.width - buttonWidth, localBottomRect.y, buttonWidth, localBottomRect.height);
				Widgets.ButtonText(btnApplyRect, "Apply");
				Rect btnCancelRect = new Rect(btnApplyRect.x - kMargin - buttonWidth, localBottomRect.y, buttonWidth, localBottomRect.height);
				Widgets.ButtonText(btnCancelRect, "Cancel");
				Rect btnResetRect = new Rect(btnCancelRect.x - kMargin - buttonWidth, localBottomRect.y, buttonWidth, localBottomRect.height);
				Widgets.ButtonText(btnResetRect, "Reset");
			}

			GUI.EndGroup();
		}
	}
}
