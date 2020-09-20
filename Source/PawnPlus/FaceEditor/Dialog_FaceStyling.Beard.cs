using FacialStuff.DefOfs;
using FacialStuff.Defs;
using FacialStuff.FaceEditor.ColorPicker;
using FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs;
using FacialStuff.FaceEditor.UI.Util;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff.FaceEditor
{
    public partial class Dialog_FaceStyling
    {
        public void DrawTypeSelector(Rect rect)
        {
            float editorLeft = rect.x;
            float editorTop = 30f + WidgetUtil.SelectionRowHeight;
            const float editorWidth = 325f;

            float top = editorTop + 64f;

            BodyTypeSelectionDto dresserDtoBodyTypeSelectionDto = this.DresserDto.BodyTypeSelectionDto;
            if(dresserDtoBodyTypeSelectionDto != null)
            {
                WidgetUtil.AddSelectorWidget(
                    editorLeft,
                    top,
                    editorWidth,
                    "FacialStuffEditor.BodyType".Translate() + ":", dresserDtoBodyTypeSelectionDto);
            }

            top += WidgetUtil.SelectionRowHeight + 20f;
            HeadTypeSelectionDto dresserDtoHeadTypeSelectionDto = this.DresserDto.HeadTypeSelectionDto;
            if(dresserDtoHeadTypeSelectionDto != null)
                WidgetUtil.AddSelectorWidget(
                    editorLeft,
                    top,
                    editorWidth,
                    "FacialStuffEditor.HeadType".Translate() + ":", dresserDtoHeadTypeSelectionDto);

            top += WidgetUtil.SelectionRowHeight + 20f;

            if(Controller.settings.ShowGenderAgeChange)
            {
                GUI.Label(
                          new Rect(editorLeft, top, editorWidth, 64f),
                          "FacialStuffEditor.GenderChangeWarning".Translate());

                top += 64f + 20f;

                GenderSelectionDto dresserDtoGenderSelectionDto = this.DresserDto.GenderSelectionDto;
                if(dresserDtoGenderSelectionDto != null)
                    WidgetUtil.AddSelectorWidget(
                        editorLeft,
                        top,
                        editorWidth,
                        "FacialStuffEditor.Gender".Translate() + ":", dresserDtoGenderSelectionDto);
            }

            GUI.color = Color.white;
        }

        public void DrawBeardPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();

            void FullBeards()
            {
                this._beardTab = BeardTab.FullBeards;
            }

            TabRecord item = new TabRecord(
                "FacialStuffEditor.FullBeards".Translate(),
                FullBeards,
                this._beardTab == BeardTab.FullBeards);
            list.Add(item);

            void CombinableBeards()
            {
                _beardTab = BeardTab.Combinable;
            }

            TabRecord item2 = new TabRecord(
                "FacialStuffEditor.Combinable".Translate(),
                CombinableBeards,
                _beardTab == BeardTab.Combinable);
            list.Add(item2);

            TabDrawer.DrawTabs(rect, list);

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;

            // 12 columns as base
            int divider = 3;
            int iconSides = 2;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            int rowsBeard = Mathf.CeilToInt(FullBeardDefs.Count / (float)thisColumns);
            int rowsTache = Mathf.CeilToInt(MoustacheDefs.Count / (float)thisColumns);
            int rowsLowerBeards = Mathf.CeilToInt(LowerBeardDefs.Count / (float)thisColumns);

            int allRows;

            if(_beardTab == BeardTab.Combinable)
            {
                allRows = rowsTache + rowsLowerBeards;
            } else
            {
                allRows = rowsBeard;
            }

            rect3.height = allRows * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * 2, thisEntrySize);
            if(rect3.height > rect2.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * allRows;
            }

            switch(this._beardTab)
            {
                case BeardTab.Combinable:
                    Widgets.BeginScrollView(rect2, ref this._scrollPositionBeard1, rect3);
                    break;

                case BeardTab.FullBeards:
                    Widgets.BeginScrollView(rect2, ref this._scrollPositionBeard2, rect3);
                    break;
            }

            GUI.BeginGroup(rect3);

            float curY = 0f;
            float thisY = 0f;
            if(this._beardTab == BeardTab.Combinable)
            {
                for(int i = 0; i < MoustacheDefs.Count; i++)
                {
                    int yPos = i / thisColumns;
                    int xPos = i % thisColumns;

                    Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                    this.DrawMoustachePickerCell(MoustacheDefs[i], rect4.ContractedBy(3f));
                    thisY = rect4.yMax;
                }

                curY = thisY;
                for(int i = 0; i < LowerBeardDefs.Count; i++)
                {
                    int num2 = i / thisColumns;
                    int num3 = i % thisColumns;

                    Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y + curY, vector.x, vector.y);
                    this.DrawBeardPickerCell(LowerBeardDefs[i], rect4.ContractedBy(3f));
                }
            }

            if(this._beardTab == BeardTab.FullBeards)
            {
                for(int i = 0; i < FullBeardDefs.Count; i++)
                {
                    int num2 = i / thisColumns;
                    int num3 = i % thisColumns;

                    Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                    this.DrawBeardPickerCell(FullBeardDefs[i], rect4.ContractedBy(3f));
                }
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawBeardColorPickerCell(Color color, Rect rect, string colorName)
        {
            Widgets.DrawBoxSolid(rect, color);
            string text = colorName;
            Widgets.DrawHighlightIfMouseover(rect);
            if(color == NewBeardColor)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if(Widgets.ButtonInvisible(rect))
            {
                this.RemoveColorPicker();

                this._colourWrapper.Color = color;
                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        this._colourWrapper,
                        delegate
                        {
                            if(this.PawnFace.HasSameBeardColor)
                            {
                                this.NewHairColor = this._colourWrapper.Color;
                            }

                            this.NewBeardColor = this._colourWrapper.Color;
                        },
                        false,
                        true)
                    {
                        initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                    });
            }
        }

        private void DrawBeardPickerCell(BeardDef beard, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = beard.LabelCap;
            float offset = (rect.width / 2 - rect.height) / 3;
            {
                // Highlight box
                Widgets.DrawHighlightIfMouseover(rect);
                if(beard == this.NewBeard)
                {
                    Widgets.DrawHighlightSelected(rect);
                    text += "\n(selected)";
                } else
                {
                    if(beard == this._originalBeard)
                    {
                        Widgets.DrawAltRect(rect);
                        text += "\n(original)";
                    }
                }
            }

            if(this._newMoustache != MoustacheDefOf.Shaved)
            {
                {
                    // if (beard.beardType == BeardType.FullBeard)
                    // {
                    // Widgets.DrawBoxSolid(rect, new Color(0.8f, 0f, 0f, 0.3f));
                    // }
                    // else
                    if(this.NewBeard == BeardDefOf.Beard_Shaved || this.NewMoustache != MoustacheDefOf.Shaved)
                    {
                        Widgets.DrawBoxSolid(rect, new Color(0.29f, 0.7f, 0.8f, 0.3f));
                    } else
                    {
                        Widgets.DrawBoxSolid(rect, new Color(0.8f, 0.8f, 0.8f, 0.3f));
                    }
                }
            }

            // Get the offset, cause width != 2 * height
            Rect leftRect = new Rect(rect.x + offset, rect.y, rect.height, rect.height);
            Rect rightRect = new Rect(leftRect.xMax + offset, rect.y, rect.height, rect.height);

            GUI.color = Pawn.story.SkinColor;
            GUI.DrawTexture(leftRect, Pawn.Drawer.renderer.graphics.headGraphic.MatSouth.mainTexture);
            GUI.DrawTexture(rightRect, Pawn.Drawer.renderer.graphics.headGraphic.MatEast.mainTexture);

            // Draw hair if mouse is over
            GUI.color = Pawn.story.hairColor;

            // if (Mouse.IsOver(leftRect) || Mouse.IsOver(rightRect))
            // {
            // GUI.DrawTexture(leftRect, pawn.Drawer.renderer.graphics.hairGraphic.MatSouth.mainTexture);
            // GUI.DrawTexture(rightRect, pawn.Drawer.renderer.graphics.hairGraphic.MatEast.mainTexture);
            // }

            // Draw selected beard
            GUI.color = this.PawnFace.HasSameBeardColor ? Pawn.story.hairColor : this.NewBeardColor;
            GUI.DrawTexture(leftRect, this.BeardGraphic(beard)?.MatSouth.mainTexture);
            GUI.DrawTexture(rightRect, this.BeardGraphic(beard)?.MatEast.mainTexture);
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if(Widgets.ButtonInvisible(rect))
            {
                this.NewBeard = beard;
                this.DoColorWindowBeard();
            }
        }
    }
}
