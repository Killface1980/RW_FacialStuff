using PawnPlus.FaceEditor.ColorPicker;
using PawnPlus.Genetics;
using PawnPlus.Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.FaceEditor
{
	public partial class Dialog_FaceStyling
	{
        public virtual void DrawHairPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                "Female".Translate(),
                delegate
                {
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x =>
                            x.hairTags.SharesElementWith(VanillaHairTags) &&
                            (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually && !x.IsBeardNotHair()));
                    HairDefs.SortBy(i => i.LabelCap.ToString());
                    genderTab = GenderTab.Female;
                }, 
                genderTab == GenderTab.Female);
            list.Add(item);

            TabRecord item2 = new TabRecord(
                "Male".Translate(),
                delegate
                {
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x =>
                            x.hairTags.SharesElementWith(VanillaHairTags) &&
                            (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually && !x.IsBeardNotHair()));
                    HairDefs.SortBy(i => i.LabelCap.ToString());
                    genderTab = GenderTab.Male;
                }, 
                genderTab == GenderTab.Male);
            list.Add(item2);

            TabRecord item3 = new TabRecord(
                "FacialStuffEditor.Any".Translate(),
                delegate
                {
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x =>
                            x.hairTags.SharesElementWith(VanillaHairTags) &&
                            x.hairGender == HairGender.Any && !x.IsBeardNotHair());
                    HairDefs.SortBy(i => i.LabelCap.ToString());
                    genderTab = GenderTab.Any;
                }, genderTab == GenderTab.Any);
            list.Add(item3);

            TabRecord item4 = new TabRecord(
                "FacialStuffEditor.All".Translate(),
                delegate
                {
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairTags.SharesElementWith(VanillaHairTags) && !x.IsBeardNotHair());
                    HairDefs.SortBy(i => i.LabelCap.ToString());
                    genderTab = GenderTab.All;
                }, 
                genderTab == GenderTab.All);

            list.Add(item4);

            TabDrawer.DrawTabs(rect, list);

            List<TabRecord> list2 = new List<TabRecord>();

            TabRecord urban = new TabRecord(
                "FacialStuffEditor.Urban".Translate(),
                delegate
                {
                    CurrentFilter = new List<string> { "Urban" };
                    this.filterTab = FilterTab.Urban;
                }, 
                this.filterTab == FilterTab.Urban);
            list2.Add(urban);

            TabRecord rural = new TabRecord(
                "FacialStuffEditor.Rural".Translate(),
                delegate
                {
                    CurrentFilter = new List<string> { "Rural" };
                    this.filterTab = FilterTab.Rural;
                }, 
                this.filterTab == FilterTab.Rural);
            list2.Add(rural);

            TabRecord punk = new TabRecord(
                "FacialStuffEditor.Punk".Translate(),
                delegate
                {
                    CurrentFilter = new List<string> { "Punk" };
                    this.filterTab = FilterTab.Punk;
                }, 
                this.filterTab == FilterTab.Punk);
            list2.Add(punk);

            TabRecord tribal = new TabRecord(
                "FacialStuffEditor.Tribal".Translate(),
                delegate
                {
                    CurrentFilter = new List<string> { "Tribal" };
                    this.filterTab = FilterTab.Tribal;
                }, 
                this.filterTab == FilterTab.Tribal);
            list2.Add(tribal);

            Rect rect2A = new Rect(rect);

            rect2A.yMin += 32f;

            TabDrawer.DrawTabs(rect2A, list2);

            Rect rect2 = rect2A.ContractedBy(1f);
            Rect rect3 = rect2;

            // 12 columns as base
            int divider = 3;
            int iconSides = 2;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            int rowsCount = Mathf.CeilToInt(FilteredHairDefs.Count / (float)thisColumns);

            rect3.height = rowsCount * thisEntrySize;

            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if(rect3.height > rect2.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * rowsCount;
            }

            switch(genderTab)
            {
                case GenderTab.Male:
                    Widgets.BeginScrollView(rect2, ref ScrollPositionHairMale, rect3);
                    break;

                case GenderTab.Female:
                    Widgets.BeginScrollView(rect2, ref ScrollPositionHairFemale, rect3);
                    break;

                case GenderTab.Any:
                    Widgets.BeginScrollView(rect2, ref ScrollPositionHairAny, rect3);
                    break;

                case GenderTab.All:
                    Widgets.BeginScrollView(rect2, ref ScrollPositionHairAll, rect3);
                    break;
            }

            GUI.BeginGroup(rect3);

            for(int i = 0; i < FilteredHairDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                DrawHairPickerCell(FilteredHairDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        public void DrawHairPickerCell(HairDef hair, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string label = hair.LabelCap;

            // Get the offset, cause width != 2 * height
            float offset = (rect.width / 2 - rect.height) / 3;

            Rect rect1 = new Rect(rect.x + offset, rect.y, rect.height, rect.height);
            Rect rect2 = new Rect(rect1.xMax + offset, rect.y, rect.height, rect.height);
            {
                // Highlight box
                Widgets.DrawHighlightIfMouseover(rect);
                if(hair == NewHair)
                {
                    Widgets.DrawHighlightSelected(rect);
                    label += "\n(selected)";
                } else
                {
                    if(hair == _originalHair)
                    {
                        Widgets.DrawAltRect(rect);
                        label += "\n(original)";
                    }
                }
            }

            string highlightText = string.Empty;
            foreach(string hairTag in hair.hairTags)
            {
                if(!highlightText.NullOrEmpty())
                {
                    highlightText += "\n";
                }

                highlightText += hairTag;
            }

            // Rect rect3 = new Rect(rect2.xMax, rect.y, rect.height, rect.height);
            GUI.color = Pawn.story.SkinColor;
            GUI.DrawTexture(rect1, Pawn.Drawer.renderer.graphics.headGraphic.MatSouth.mainTexture);
            GUI.DrawTexture(rect2, Pawn.Drawer.renderer.graphics.headGraphic.MatEast.mainTexture);

            GUI.color = Pawn.story.hairColor;
            GUI.DrawTexture(rect1, HairGraphic(hair).MatSouth.mainTexture);
            GUI.DrawTexture(rect2, HairGraphic(hair).MatEast.mainTexture);

            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, highlightText);

            // ReSharper disable once InvertIf
            if(Widgets.ButtonInvisible(rect))
            {
                this.NewHair = hair;

                this.RemoveColorPicker();
                {
                    this._colourWrapper.Color = this.NewHairColor;

                    Find.WindowStack.Add(
                        new Dialog_ColorPicker(
                            this._colourWrapper,
                            delegate { this.NewHairColor = this._colourWrapper.Color; },
                            false,
                            true)
                            {
                                initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                            });
                }
            }
        }

        public void DrawHairColorPickerCell(
            Color color,
            Rect rect,
            string colorName,
            HairColorRequest colorRequest = null)
        {
            string text = colorName;
            Widgets.DrawHighlightIfMouseover(rect);
            if(this.NewHairColor.IndistinguishableFrom(color))
            {
                this.DrawColorSelected(rect.ContractedBy(-2f));
                text += "\n(selected)";
            }

            Widgets.DrawBoxSolid(rect, color);

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if(Widgets.ButtonInvisible(rect))
            {
                this.RemoveColorPicker();

                if(colorRequest != null)
                {
                    this.PawnFace.PheoMelanin = colorRequest.PheoMelanin;
                    this.PawnFace.EuMelanin = colorRequest.EuMelanin;
                }

                this._colourWrapper.Color = color;
                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        this._colourWrapper,
                        delegate { this.NewHairColor = this._colourWrapper.Color; },
                        false,
                        true)
                        {
                            initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                        });
            }
        }
    }
}
