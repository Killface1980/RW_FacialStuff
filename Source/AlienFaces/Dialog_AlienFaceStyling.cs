using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienFace
{

    using FacialStuff.Defs;
    using FacialStuff.FaceEditor;
    using FacialStuff.FaceEditor.UI.DTO;
    using FacialStuff.FaceEditor.UI.Util;
    using FacialStuff.Genetics;
    using FacialStuff.Utilities;

    using global::AlienRace;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    using Controller = FacialStuff.Controller;

    [StaticConstructorOnStartup]
    public class Dialog_AlienFaceStyling : Dialog_FaceStyling
    {

        private readonly AlienRace alienRace;

        public Dialog_AlienFaceStyling(Pawn p, ThingDef_AlienRace alienProp) : base(p)
        {
            PawnColorUtils.InitializeColors();
            this.alienRace = ProviderAlienRaces.GetAlienRace(alienProp, p);
            if (this.alienRace.HasHair)
            {
                HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                    x => x.hairTags.SharesElementWith(this.alienRace.HairTags));
                CurrentFilter = this.alienRace.HairTags;

            }
            this.useSkincolorForHair = ((ThingDef_AlienRace)this.compFace.Pawn.def).alienRace.generalSettings.alienPartGenerator
                .useSkincolorForHair;
            this.genderTab = GenderTab.All;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(MarginFS, 0f, inRect.width, TitleHeight);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, Title);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // re-render pawn
            try
            {
                if (this.rerenderPawn)
                {
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(pawn);
                    this.rerenderPawn = false;
                }

                if (!this.initialized)
                {
                    this.dresserDto = new DresserDTO(pawn);
                    this.dresserDto.SetUpdatePawnListeners(this.UpdatePawn);
                }
            }
            catch (Exception ex)
            {
            }

            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                "FacialStuffEditor.Hair".Translate(),
                this.SetTabFaceStyle(FaceStyleTab.Hair),
                this.tab == FaceStyleTab.Hair);
            list.Add(item);

            if (this.compFace.Props.hasBeard)
            {
                if (pawn.gender == Gender.Male)
                {
                    TabRecord item2 = new TabRecord(
                        "FacialStuffEditor.Beard".Translate(),
                        this.SetTabFaceStyle(FaceStyleTab.Beard),
                        this.tab == FaceStyleTab.Beard);
                    list.Add(item2);
                }
            }

            if (this.compFace.Props.hasEyes)
            {
                TabRecord item3 = new TabRecord(
                    "FacialStuffEditor.Eye".Translate(),
                    this.SetTabFaceStyle(FaceStyleTab.Eye),
                    this.tab == FaceStyleTab.Eye);
                list.Add(item3);

                TabRecord item4 = new TabRecord(
                    "FacialStuffEditor.Brow".Translate(),
                    this.SetTabFaceStyle(FaceStyleTab.Brow),
                    this.tab == FaceStyleTab.Brow);
                list.Add(item4);
            }

            if (Controller.settings.ShowBodyChange)
            {
                TabRecord item5 = new TabRecord(
                    "FacialStuffEditor.TypeSelector".Translate(),
                    this.SetTabFaceStyle(FaceStyleTab.TypeSelector),
                    this.tab == FaceStyleTab.TypeSelector);
                list.Add(item5);
            }

            Rect contentRect = new Rect(
                0f,
                TitleHeight + TabDrawer.TabHeight + MarginFS / 2,
                inRect.width,
                inRect.height - TitleHeight - MarginFS * 2 - TabDrawer.TabHeight);

            TabDrawer.DrawTabs(contentRect, list);

            this.DrawUI(contentRect);

            Action backAct = delegate
                {
                    this.RemoveColorPicker();

                    // SoundDef.Named("InteractShotgun").PlayOneShotOnCamera();
                    if (this.originalGender != Gender.Male && this.tab == FaceStyleTab.Beard)
                    {
                        this.tab = FaceStyleTab.Hair;
                    }

                    this.ResetPawnFace();
                };

            DialogUtility.DoNextBackButtons(
                inRect,
                "Randomize".Translate(),
                "FacialStuffEditor.Accept".Translate(),
                backAct,
                this.FaceRandomizer,
                this.SaveAndClose);
        }
        // todo: rewrite
        protected static Vector2 SwatchPosition = new Vector2(18, 320);
        protected static Vector2 SwatchSize = new Vector2(15, 15);
        protected static Vector2 SwatchSpacing = new Vector2(21, 21);
        protected static float SwatchLimit = 210;

        private bool useSkincolorForHair;

        protected void DrawHumanlikeColorSelector(float cursorY)
        {
            int currentSwatchIndex = PawnColorUtils.GetLeftIndexForValue(this.NewMelanin);
            Color currentSwatchColor = PawnColorUtils.Colors[currentSwatchIndex];

            Rect swatchRect = new Rect(SwatchPosition.x, cursorY, SwatchSize.x, SwatchSize.y);

            // Draw the swatch selection boxes.
            int colorCount = PawnColorUtils.Colors.Length - 1;
            int clickedIndex = -1;
            for (int i = 0; i < colorCount; i++)
            {
                Color color = PawnColorUtils.Colors[i];

                // If the swatch is selected, draw a heavier border around it.
                bool isThisSwatchSelected = (i == currentSwatchIndex);
                if (isThisSwatchSelected)
                {
                    Rect selectionRect = new Rect(swatchRect.x - 2, swatchRect.y - 2, SwatchSize.x + 4, SwatchSize.y + 4);
                    GUI.color = ColorSwatchSelection;
                    GUI.DrawTexture(selectionRect, BaseContent.WhiteTex);
                }

                // Draw the border around the swatch.
                Rect borderRect = new Rect(swatchRect.x - 1, swatchRect.y - 1, SwatchSize.x + 2, SwatchSize.y + 2);
                GUI.color = ColorSwatchBorder;
                GUI.DrawTexture(borderRect, BaseContent.WhiteTex);

                // Draw the swatch itself.
                GUI.color = color;
                GUI.DrawTexture(swatchRect, BaseContent.WhiteTex);

                if (!isThisSwatchSelected)
                {
                    if (Widgets.ButtonInvisible(swatchRect, false))
                    {
                        clickedIndex = i;
                        //currentSwatchColor = color;
                    }
                }

                // Advance the swatch rect cursor position and wrap it if necessary.
                swatchRect.x += SwatchSpacing.x;
                if (swatchRect.x >= SwatchLimit - SwatchSize.x)
                {
                    swatchRect.y += SwatchSpacing.y;
                    swatchRect.x = SwatchPosition.x;
                }
            }

            // Draw the current color box.
            GUI.color = Color.white;
            Rect currentColorRect = new Rect(SwatchPosition.x, swatchRect.y + 4, 49, 49);
            if (swatchRect.x != SwatchPosition.x)
            {
                currentColorRect.y += SwatchSpacing.y;
            }
            GUI.color = ColorSwatchBorder;
            GUI.DrawTexture(currentColorRect, BaseContent.WhiteTex);
            GUI.color = pawn.story.SkinColor;
            GUI.DrawTexture(currentColorRect.ContractedBy(1), BaseContent.WhiteTex);
            GUI.color = Color.white;

            // Figure out the lerp value so that we can draw the slider.
            float minValue = 0.00f;
            float maxValue = 0.99f;
            float t = PawnColorUtils.GetRelativeLerpValue(this.NewMelanin);
            if (t < minValue)
            {
                t = minValue;
            }
            else if (t > maxValue)
            {
                t = maxValue;
            }
            if (clickedIndex != -1)
            {
                t = minValue;
            }

            // Draw the slider.
            float newValue = GUI.HorizontalSlider(new Rect(currentColorRect.x + 56, currentColorRect.y + 18, 136, 16), t, minValue, 1);
            if (newValue < minValue)
            {
                newValue = minValue;
            }
            else if (newValue > maxValue)
            {
                newValue = maxValue;
            }
            GUI.color = Color.white;

            // If the user selected a new swatch or changed the lerp value, set a new color value.
            if (t != newValue || clickedIndex != -1)
            {
                if (clickedIndex != -1)
                {
                    currentSwatchIndex = clickedIndex;
                }
                float melaninLevel = PawnColorUtils.GetValueFromRelativeLerp(currentSwatchIndex, newValue);
                this.NewMelanin = melaninLevel;
                this.rerenderPawn = true;
            }
        }

        protected void DrawAlienPawnColorSelector(float cursorY, List<Color> colors, bool allowAnyColor)
        {
            Color currentColor = pawn.story.SkinColor;
            Color clickedColor = currentColor;
            Rect rect = new Rect(SwatchPosition.x, cursorY, SwatchSize.x, SwatchSize.y);
            foreach (Color color in colors)
            {
                bool selected = (color == currentColor);
                if (selected)
                {
                    Rect selectionRect = new Rect(rect.x - 2, rect.y - 2, SwatchSize.x + 4, SwatchSize.y + 4);
                    GUI.color = ColorSwatchSelection;
                    GUI.DrawTexture(selectionRect, BaseContent.WhiteTex);
                }

                Rect borderRect = new Rect(rect.x - 1, rect.y - 1, SwatchSize.x + 2, SwatchSize.y + 2);
                GUI.color = ColorSwatchBorder;
                GUI.DrawTexture(borderRect, BaseContent.WhiteTex);

                GUI.color = color;
                GUI.DrawTexture(rect, BaseContent.WhiteTex);

                if (!selected)
                {
                    if (Widgets.ButtonInvisible(rect, false))
                    {
                        clickedColor = color;
                    }
                }

                rect.x += SwatchSpacing.x;
                if (rect.x >= SwatchLimit - SwatchSize.x)
                {
                    rect.y += SwatchSpacing.y;
                    rect.x = SwatchPosition.x;
                }
            }

            GUI.color = Color.white;
            if (!allowAnyColor)
            {
                return;
            }

            if (rect.x != SwatchPosition.x)
            {
                rect.x = SwatchPosition.x;
                rect.y += SwatchSpacing.y;
            }
            rect.y += 4;
            rect.width = 49;
            rect.height = 49;
            GUI.color = ColorSwatchBorder;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = currentColor;
            GUI.DrawTexture(rect.ContractedBy(1), BaseContent.WhiteTex);

            float originalR = currentColor.r;
            float originalG = currentColor.g;
            float originalB = currentColor.b;
            GUI.color = Color.red;
            float r = GUI.HorizontalSlider(new Rect(rect.x + 56, rect.y - 1, 136, 16), currentColor.r, 0, 1);
            GUI.color = Color.green;
            float g = GUI.HorizontalSlider(new Rect(rect.x + 56, rect.y + 19, 136, 16), currentColor.g, 0, 1);
            GUI.color = Color.blue;
            float b = GUI.HorizontalSlider(new Rect(rect.x + 56, rect.y + 39, 136, 16), currentColor.b, 0, 1);
            if (!CloseEnough(r, originalR) || !CloseEnough(g, originalG) || !CloseEnough(b, originalB))
            {
                clickedColor = new Color(r, g, b);
            }

            GUI.color = Color.white;

            if (clickedColor != currentColor)
            {
                var comp = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
                comp.skinColor = clickedColor;

                if (this.useSkincolorForHair)
                {
                    this.NewHairColor = clickedColor;
                }
                this.rerenderPawn = true;
            }
        }

        protected bool CloseEnough(float a, float b)
        {
            if (a > b - 0.0001f && a < b + 0.0001f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void DrawSkinColorSelector(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.3f, 0.3f, 0.3f));

            if (this.alienRace.UseMelaninLevels)
            {
                this.DrawHumanlikeColorSelector(rect.y + 5f);
            }
            else
            {
                this.DrawAlienPawnColorSelector(rect.y + 5f, this.alienRace.PrimaryColors, true);
            }


        }

        public override void DrawHairPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();
            var hairTags = this.alienRace.HairTags;
            if (this.compFace.Pawn.def.race.hasGenders)
            {

                TabRecord item = new TabRecord(
                    "Female".Translate(),
                    delegate
                        {
                            HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                x => x.hairTags.SharesElementWith(hairTags)
                                     && (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
                            HairDefs.SortBy(i => i.LabelCap);
                            this.genderTab = GenderTab.Female;
                        },
                    this.genderTab == GenderTab.Female);
                list.Add(item);

                TabRecord item2 = new TabRecord(
                    "Male".Translate(),
                    delegate
                        {
                            HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                x => x.hairTags.SharesElementWith(hairTags)
                                     && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                            HairDefs.SortBy(i => i.LabelCap);
                            this.genderTab = GenderTab.Male;
                        },
                    this.genderTab == GenderTab.Male);
                list.Add(item2);

                TabRecord item3 = new TabRecord(
                    "FacialStuffEditor.Any".Translate(),
                    delegate
                        {
                            HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                x => x.hairTags.SharesElementWith(hairTags) && x.hairGender == HairGender.Any);
                            HairDefs.SortBy(i => i.LabelCap);
                            this.genderTab = GenderTab.Any;
                        },
                    this.genderTab == GenderTab.Any);
                list.Add(item3);
            }

            TabRecord item4 = new TabRecord(
                "FacialStuffEditor.All".Translate(),
                delegate
                    {
                        HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(hairTags));
                        HairDefs.SortBy(i => i.LabelCap);
                        this.genderTab = GenderTab.All;
                    },
                this.genderTab == GenderTab.All);

            list.Add(item4);

            TabDrawer.DrawTabs(rect, list);

            Rect rect2a = new Rect(rect);

            rect2a.yMin += 32f;

            Rect rect2 = rect2a.ContractedBy(1f);
            Rect rect3 = rect2;

            // 12 columns as base
            int divider = 3;
            int iconSides = 2;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            int rowsCount = Mathf.CeilToInt(filteredHairDefs.Count / (float)thisColumns);

            rect3.height = rowsCount * thisEntrySize;

            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * rowsCount;
            }

            switch (this.genderTab)
            {
                case GenderTab.Male:
                    Widgets.BeginScrollView(rect2, ref this.scrollPositionHairMale, rect3);
                    break;

                case GenderTab.Female:
                    Widgets.BeginScrollView(rect2, ref this.scrollPositionHairFemale, rect3);
                    break;

                case GenderTab.Any:
                    Widgets.BeginScrollView(rect2, ref this.scrollPositionHairAny, rect3);
                    break;

                case GenderTab.All:
                    Widgets.BeginScrollView(rect2, ref this.scrollPositionHairAll, rect3);
                    break;
            }
            GUI.BeginGroup(rect3);

            for (int i = 0; i < filteredHairDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                this.DrawHairPickerCell(filteredHairDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }


        public override void DrawUI(Rect rect)
        {
            GUI.BeginGroup(rect);
            string pawnName = pawn.NameStringShort;
            Vector2 vector = Text.CalcSize(pawnName);

            Rect pawnRect = AddPortraitWidget(0f, TitleHeight);
            Rect labelRect = new Rect(0f, pawnRect.yMax, vector.x, vector.y);
            labelRect = labelRect.CenteredOnXIn(pawnRect);

            float width = rect.width - ListWidth - MarginFS;

            Rect button = new Rect(0f, labelRect.yMax + MarginFS / 2, (width - MarginFS) / 2, WidgetUtil.SelectionRowHeight);
            Rect mainRect = new Rect(0f, button.yMax + MarginFS, width, 65f);
            if (Widgets.ButtonText(button, "FacialStuffEditor.SkinSettings".Translate()))
            {
                this.RemoveColorPicker();

                this.skinPage = true;
            }

            button.x = button.xMax + MarginFS;

            if (Widgets.ButtonText(button, "FacialStuffEditor.HairSettings".Translate()))
            {
                if (this.tab == FaceStyleTab.Beard)
                {
                    this.DoColorWindowBeard();
                }

                this.skinPage = false;
            }

            float height = rect.height - MarginFS * 3 - TitleHeight;

            Rect listRect = new Rect(0f, TitleHeight, ListWidth, height) { x = mainRect.xMax + MarginFS };

            mainRect.yMax = listRect.yMax;

            this.pickerPosition = new Vector2(mainRect.position.x, mainRect.position.y);
            this.pickerSize = new Vector2(mainRect.width, mainRect.height);

            GUI.DrawTexture(
                new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height),
                NameBackground);
            Widgets.Label(labelRect, pawnName);

            Rect set = new Rect(mainRect) { height = WidgetUtil.SelectionRowHeight, width = mainRect.width / 2 - 10f };
            set.y = listRect.yMax - WidgetUtil.SelectionRowHeight;
            set.width = mainRect.width - MarginFS / 3;

            bool faceCompDrawMouth = this.compFace.PawnFace.DrawMouth;
            bool faceCompHasSameBeardColor = this.compFace.PawnFace.HasSameBeardColor;

            mainRect.yMax -= WidgetUtil.SelectionRowHeight + MarginFS;
            if (this.skinPage)
            {
                this.DrawSkinColorSelector(mainRect);
                if (Controller.settings.UseMouth)
                {
                    Widgets.CheckboxLabeled(set, "FacialStuffEditor.DrawMouth".Translate(), ref faceCompDrawMouth);
                }
            }
            else
            {
                if (this.tab == FaceStyleTab.Beard && !faceCompHasSameBeardColor)
                {
                }
                else
                {
                    if (HairDefs != null && HairDefs.Count > 0)
                    {
                        DrawColorSelector(mainRect, this.alienRace.HairColors, true);
                    }

                    if (pawn.gender == Gender.Male)
                    {
                        Widgets.CheckboxLabeled(
                            set,
                            "FacialStuffEditor.SameColor".Translate(),
                            ref faceCompHasSameBeardColor);
                        TooltipHandler.TipRegion(set, "FacialStuffEditor.SameColorTip".Translate());
                    }
                }

                if (this.tab == FaceStyleTab.Hair || this.tab == FaceStyleTab.Beard)
                {
                    listRect.yMin += TabDrawer.TabHeight;
                }

                Widgets.DrawMenuSection(listRect);

                // if (Widgets.ButtonText(set, "SelectFacePreset".Translate(), true, false))
                // {
                // var list = new List<FloatMenuOption>();
                // foreach (var current in Current.Game.outfitDatabase.AllOutfits)
                // {
                // var localOut = current;
                // list.Add(new FloatMenuOption(localOut.label, delegate { SelectedFacePreset = localOut; },
                // MenuOptionPriority.Medium, null, null));
                // }
                // Find.WindowStack.Add(new FloatMenu(list));
                // }
                if (GUI.changed)
                {
                    if (this.compFace.PawnFace.HasSameBeardColor != faceCompHasSameBeardColor)
                    {
                        this.RemoveColorPicker();
                        this.compFace.PawnFace.HasSameBeardColor = faceCompHasSameBeardColor;
                        this.NewBeardColor = HairMelanin.DarkerBeardColor(this.NewHairColor);
                    }
                }

                if (this.compFace.PawnFace.DrawMouth != faceCompDrawMouth)
                {
                    this.compFace.PawnFace.DrawMouth = faceCompDrawMouth;
                    this.rerenderPawn = true;
                }

                set.width = mainRect.width / 2 - 10f;

                set.y += 36f;
                set.x = mainRect.x;

                if (this.tab == FaceStyleTab.Eye)
                {
                    this.DrawEyePicker(listRect);
                }

                if (this.tab == FaceStyleTab.Brow)
                {
                    if (pawn.gender == Gender.Female)
                    {
                        browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                            x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                        browDefs.SortBy(i => i.LabelCap);
                    }

                    if (pawn.gender == Gender.Male)
                    {
                        browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                            x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                        browDefs.SortBy(i => i.LabelCap);
                    }

                    this.DrawBrowPicker(listRect);
                }

                if (this.tab == FaceStyleTab.Hair)
                {
                    this.DrawHairPicker(listRect);
                }

                if (this.tab == FaceStyleTab.Beard)
                {
                    this.DrawBeardPicker(listRect);
                }

                if (this.tab == FaceStyleTab.TypeSelector)
                {
                    this.DrawTypeSelector(listRect);
                }

            }
            GUI.EndGroup();

        }

        protected void DrawColorSelector(Rect crect, List<Color> colors, bool allowAnyColor)
        {

            Color currentColor = this.NewHairColor;
            Rect rect = new Rect(SwatchPosition.x, crect.y + 5f, SwatchSize.x, SwatchSize.y);
            if (colors != null)
            {
                foreach (Color color in colors)
                {
                    bool selected = (color == currentColor);
                    if (selected)
                    {
                        Rect selectionRect = new Rect(rect.x - 2, rect.y - 2, SwatchSize.x + 4, SwatchSize.y + 4);
                        GUI.color = ColorSwatchSelection;
                        GUI.DrawTexture(selectionRect, BaseContent.WhiteTex);
                    }

                    Rect borderRect = new Rect(rect.x - 1, rect.y - 1, SwatchSize.x + 2, SwatchSize.y + 2);
                    GUI.color = ColorSwatchBorder;
                    GUI.DrawTexture(borderRect, BaseContent.WhiteTex);

                    GUI.color = color;
                    GUI.DrawTexture(rect, BaseContent.WhiteTex);

                    if (!selected)
                    {
                        if (Widgets.ButtonInvisible(rect, false))
                        {
                            NewHairColor = (color);
                            currentColor = color;

                            if (useSkincolorForHair)
                            {
                                var comp = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
                                comp.skinColor = currentColor;
                            }
                            this.rerenderPawn = true;
                        }
                    }

                    rect.x += SwatchSpacing.x;
                    if (rect.x >= SwatchLimit - SwatchSize.x)
                    {
                        rect.y += SwatchSpacing.y;
                        rect.x = SwatchPosition.x;
                    }
                }
            }

            GUI.color = Color.white;
            if (!allowAnyColor)
            {
                return;
            }

            if (rect.x != SwatchPosition.x)
            {
                rect.x = SwatchPosition.x;
                rect.y += SwatchSpacing.y;
            }
            rect.y += 4;
            rect.width = 49;
            rect.height = 49;
            GUI.color = ColorSwatchBorder;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = currentColor;
            GUI.DrawTexture(rect.ContractedBy(1), BaseContent.WhiteTex);

            GUI.color = Color.red;
            float originalR = currentColor.r;
            float originalG = currentColor.g;
            float originalB = currentColor.b;
            float r = GUI.HorizontalSlider(new Rect(rect.x + 56, rect.y - 1, 136, 16), currentColor.r, 0, 1);
            GUI.color = Color.green;
            float g = GUI.HorizontalSlider(new Rect(rect.x + 56, rect.y + 19, 136, 16), currentColor.g, 0, 1);
            GUI.color = Color.blue;
            float b = GUI.HorizontalSlider(new Rect(rect.x + 56, rect.y + 39, 136, 16), currentColor.b, 0, 1);
            if (!CloseEnough(r, originalR) || !CloseEnough(g, originalG) || !CloseEnough(b, originalB))
            {
                NewHairColor = (new Color(r, g, b));
                if (useSkincolorForHair)
                {
                    var comp = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
                    comp.skinColor = this.NewHairColor;
                }
                this.rerenderPawn = true;
            }

            GUI.color = Color.white;
        }
    }
}
