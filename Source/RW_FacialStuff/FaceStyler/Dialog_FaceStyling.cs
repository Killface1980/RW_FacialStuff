using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using CommunityCoreLibrary.ColorPicker;
using RW_FacialStuff;
using RW_FacialStuff.Defs;
using UnityEngine;
using Verse;

namespace FaceStyling
{
    public class Dialog_FaceStyling : Window
    {
        public enum GraphicSlotGroup
        {
            Invalid = -1,
            Body,
            OnSkinOnLegs,
            OnSkin,
            Middle,
            Shell,
            Head,
            Eyes,
            Brows,
            Lips,
            Beard,
            Hair,
            Overhead
        }

        public class GraphicsDisp
        {
            public Thing thing = null;
            public Graphic graphic = null;
            public Texture2D icon = null;
            public Color color = Color.white;
            private float yOffset = 0f;

            public bool Valid
            {
                get
                {
                    return thing != null;
                }
            }

            public GraphicsDisp()
            {
                thing = null;
                graphic = null;
                icon = null;
                yOffset = 0f;
            }

            public GraphicsDisp(Thing thing, Graphic graphic, Texture2D icon, Color color, float yOffset)
            {
                this.thing = thing;
                this.graphic = graphic;
                this.icon = icon;
                this.color = color;
                this.yOffset = yOffset;
            }

            public void Draw(Rect rect)
            {
                if (Valid && graphic != null)
                {
                    Rect position = new Rect(rect.x, rect.y + yOffset, rect.width, rect.height);
                    GUI.color = color;
                    GUI.DrawTexture(position, graphic.MatFront.mainTexture);
                    GUI.color = Color.white;
                }
            }
            public void DrawIcon(Rect rect)
            {
                if (Valid && icon != null)
                {
                    GUI.color = color;
                    GUI.DrawTexture(rect, icon);
                    GUI.color = Color.white;
                }
            }
        }

        private static string _title;
        private static float _titleHeight;
        private static float _previewSize;
        private static float _iconSize;
        private static Texture2D _icon;
        private static float _margin;
        private static float _listWidth;
        private static int _columns;
        private static float _entrySize;
        private static Texture2D _nameBackground;
        private static List<HairDef> _hairDefs;
        private static List<BeardDef> _beardDefs;
        private static List<LipDef> _lipDefs;
        private static List<EyeDef> _eyeDefs;
        private static List<BrowDef> _browDefs;
        private static Pawn pawn;
        private static GraphicsDisp[] DisplayGraphics;
        private static HairDef _newHair;
        private static HairDef originalHair;
        private static BeardDef _newBeard;
        private static BeardDef originalBeard;
        private static LipDef _newLip;
        private static LipDef originalLip;
        private static EyeDef _newEye;
        private static EyeDef originalEye;
        private static BrowDef _newBrow;
        private static BrowDef originalBrow;
        private static Color _newColour;
        private static Color originalColour;
        private static ColorWrapper colourWrapper;
        private Vector2 _scrollPosition = Vector2.zero;

        public HairDef newHair
        {
            get
            {
                return _newHair;
            }
            set
            {
                _newHair = value;
                SetGraphicSlot(GraphicSlotGroup.Hair, pawn, HairGraphic(value), pawn.def.uiIcon, newColour);
            }
        }

        public BeardDef newBeard
        {
            get
            {
                return _newBeard;
            }
            set
            {
                _newBeard = value;
                SetGraphicSlot(GraphicSlotGroup.Beard, pawn, BeardGraphic(value), pawn.def.uiIcon, newColour);
            }
        }

        public LipDef newLip
        {
            get
            {
                return _newLip;
            }
            set
            {
                _newLip = value;
                SetGraphicSlot(GraphicSlotGroup.Lips, pawn, LipGraphic(value), pawn.def.uiIcon, newColour);
            }
        }

        public EyeDef newEye
        {
            get
            {
                return _newEye;
            }
            set
            {
                _newEye = value;
                SetGraphicSlot(GraphicSlotGroup.Eyes, pawn, EyeGraphic(value), pawn.def.uiIcon, newColour);
            }
        }

        public BrowDef newBrow
        {
            get
            {
                return _newBrow;
            }
            set
            {
                _newBrow = value;
                SetGraphicSlot(GraphicSlotGroup.Brows, pawn, BrowGraphic(value), pawn.def.uiIcon, newColour);
            }
        }

        public Color newColour
        {
            get
            {
                return _newColour;
            }
            set
            {
                _newColour = value;
                DisplayGraphics[9].color = value;
                DisplayGraphics[10].color = value;
            }
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(_previewSize + _margin + _listWidth + 36f, 40f + _previewSize * 2f + _margin * 3f + 38f + 36f);
            }
        }

        static Dialog_FaceStyling()
        {
            _title = "ClutterHairStylerTitle".Translate();
            _titleHeight = 40f;
            _previewSize = 250f;
            //       _previewSize = 100f;
            _iconSize = 24f;
            _icon = ContentFinder<Texture2D>.Get("ClothIcon");
            _margin = 6f;
            _listWidth = 450f;
            //   _listWidth = 200f;
            _columns = 5;
            _entrySize = _listWidth / (float)_columns;
            _nameBackground = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));
            _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x=> x.hairGender.Equals(pawn.gender));

            _beardDefs = DefDatabase<BeardDef>.AllDefsListForReading;
            _lipDefs = DefDatabase<LipDef>.AllDefsListForReading;
            _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(x => x.hairGender.Equals(pawn.gender));
            _browDefs = DefDatabase<BrowDef>.AllDefsListForReading;
            _hairDefs.SortBy(i => i.hairGender.ToString(), i => i.LabelCap);
            _beardDefs.SortBy(i => i.LabelCap);
        }

        public Dialog_FaceStyling(Pawn p)
        {
            var pawnSave = MapComponent_FacialStuff.GetCache(p);
            if (pawnSave.BeardDef == null)
                pawnSave.BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
            absorbInputAroundWindow = false;
            forcePause = true;
            closeOnClickedOutside = false;
            pawn = p;
            originalColour = (_newColour = pawn.story.hairColor);
            colourWrapper = new ColorWrapper(newColour);
            _newHair = (originalHair = pawn.story.hairDef);
            _newBeard = (originalBeard = pawnSave.BeardDef);
            _newLip = (originalLip = pawnSave.LipDef);
            _newEye = (originalEye = pawnSave.EyeDef);
            _newBrow = (originalBrow = pawnSave.BrowDef);

            DisplayGraphics = new GraphicsDisp[12];
            for (int i = 0; i < 12; i++)
            {
                DisplayGraphics[i] = new GraphicsDisp();
            }
            SetGraphicSlot(GraphicSlotGroup.Body, pawn, GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor), pawn.def.uiIcon, pawn.story.SkinColor);
            SetGraphicSlot(GraphicSlotGroup.Head, pawn, GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn, true, pawn.story.SkinColor), pawn.def.uiIcon, pawn.story.SkinColor);
            //   SetGraphicSlot(GraphicSlotGroup.Head, pawn, GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor), pawn.def.uiIcon, pawn.story.SkinColor);
            SetGraphicSlot(GraphicSlotGroup.Hair, pawn, HairGraphic(pawn.story.hairDef), pawn.def.uiIcon, pawn.story.hairColor);
            SetGraphicSlot(GraphicSlotGroup.Eyes, pawn, EyeGraphic(pawnSave.EyeDef), pawn.def.uiIcon, Color.black);
            SetGraphicSlot(GraphicSlotGroup.Brows, pawn, BrowGraphic(pawnSave.BrowDef), pawn.def.uiIcon, Color.black);
            SetGraphicSlot(GraphicSlotGroup.Lips, pawn, LipGraphic(pawnSave.LipDef), pawn.def.uiIcon, Color.black);
            SetGraphicSlot(GraphicSlotGroup.Beard, pawn, BeardGraphic(pawnSave.BeardDef), pawn.def.uiIcon, pawn.story.hairColor);
            foreach (Apparel current in pawn.apparel.WornApparel)
            {
                GraphicSlotGroup slotForApparel = GetSlotForApparel(current);
                if (slotForApparel != GraphicSlotGroup.Invalid)
                {
                    if (current.def.apparel.LastLayer != ApparelLayer.Overhead)
                        SetGraphicSlot(slotForApparel, current, ApparelGraphic(current.def, pawn.story.BodyType), current.def.uiIcon, current.DrawColor);
                }
            }
        }

        public override void PostOpen()
        {
            windowRect.x = windowRect.x - (windowRect.width - _margin) / 2f;

        }

        public override void PreClose()
        {
            while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker), false))
            {
            }

        }

        private Graphic HairGraphic(HairDef def)
        {
            Graphic result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi>(def.texPath, ShaderDatabase.Cutout, new Vector2(38f, 38f), Color.white, Color.white);
            }
            else
            {
                result = null;
            }
            return result;
        }

        private Graphic BeardGraphic(BeardDef def)
        {
            Graphic result;
            if (def.texPathAverageNormal != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_HeadParts>(def.texPathAverageNormal, ShaderDatabase.Cutout, new Vector2(38f, 38f), Color.white, Color.white);
            }
            else
            {
                result = null;
            }
            return result;
        }

        private Graphic EyeGraphic(EyeDef def)
        {
            Graphic result;
            if (def.texPathAverage != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_HeadParts>(def.texPathAverage, ShaderDatabase.Cutout, new Vector2(38f, 38f), Color.white, Color.white);
            }
            else
            {
                result = null;
            }
            return result;
        }

        private Graphic BrowGraphic(BrowDef def)
        {
            Graphic result;
            if (def.texPathAverage != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_HeadParts>(def.texPathAverage, ShaderDatabase.Cutout, new Vector2(38f, 38f), Color.white, Color.white);
            }
            else
            {
                result = null;
            }
            return result;
        }

        private Graphic LipGraphic(LipDef def)
        {
            Graphic result;
            if (def.texPathAverage != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_HeadParts>(def.texPathAverage, ShaderDatabase.Cutout, new Vector2(38f, 38f), Color.white, Color.white);
            }
            else
            {
                result = null;
            }
            return result;
        }

        public string Page = "hair";

        private Graphic ApparelGraphic(ThingDef def, BodyType bodyType)
        {
            Graphic result;
            if (def.apparel == null || def.apparel.wornGraphicPath.NullOrEmpty())
            {
                result = null;
            }
            else
            {
                result = GraphicDatabase.Get<Graphic_Multi>((def.apparel.LastLayer != ApparelLayer.Overhead) ? (def.apparel.wornGraphicPath + "_" + bodyType.ToString()) : def.apparel.wornGraphicPath, ShaderDatabase.Cutout, new Vector2(38f, 38f), Color.white, Color.white);
            }
            return result;
        }

        private void SetGraphicSlot(GraphicSlotGroup slotIndex, Thing newThing, Graphic newGraphic, Texture2D newIcon, Color newColor)
        {
            DisplayGraphics[(int)slotIndex] = new GraphicsDisp(newThing, newGraphic, newIcon, newColor, (slotIndex < GraphicSlotGroup.Head) ? 0f : -16f);
        }

        private GraphicSlotGroup GetSlotForApparel(Thing apparel)
        {
            ApparelProperties apparel2 = apparel.def.apparel;
            ApparelLayer lastLayer = apparel2.LastLayer;
            GraphicSlotGroup result;
            switch (lastLayer)
            {
                case ApparelLayer.OnSkin:
                    if (apparel2.bodyPartGroups.Count == 1 && apparel2.bodyPartGroups[0].Equals(BodyPartGroupDefOf.Legs))
                    {
                        result = GraphicSlotGroup.OnSkinOnLegs;
                        return result;
                    }
                    result = GraphicSlotGroup.OnSkin;
                    return result;
                case ApparelLayer.Middle:
                    result = GraphicSlotGroup.Middle;
                    return result;
                case ApparelLayer.Shell:
                    result = GraphicSlotGroup.Shell;
                    return result;
                case ApparelLayer.Overhead:
                    result = GraphicSlotGroup.Overhead;
                    return result;
            }
            Log.Warning("Could not resolve 'LastLayer' " + lastLayer.ToString());
            result = GraphicSlotGroup.Invalid;
            return result;
        }

        private void DrawUI(Rect parentRect)
        {
            GUI.BeginGroup(parentRect);
            string nameStringShort = pawn.NameStringShort;
            Vector2 vector = Text.CalcSize(nameStringShort);
            Rect pawnRect = new Rect(0f, 0f, _previewSize, _previewSize);
            Rect labelRect = new Rect(0f, pawnRect.yMax - vector.y, vector.x, vector.y);
            Rect selectionRect = new Rect(0f, pawnRect.yMax + _margin, _previewSize, _previewSize);
            Rect listRect = new Rect(_previewSize + _margin, 0f, _listWidth, parentRect.height - _margin);
            labelRect = labelRect.CenteredOnXIn(pawnRect);
            for (int i = 0; i < DisplayGraphics.Length; i++)
            {
                if (pawn.gender == Gender.Male)
                {
                    if (!newBeard.drawMouth)
                    {
                        if (i != 8)
                        {
                            if (DisplayGraphics[i].Valid)
                                DisplayGraphics[i].Draw(pawnRect);
                        }
                    }
                    else if (DisplayGraphics[i].Valid)
                    {
                        DisplayGraphics[i].Draw(pawnRect);
                    }
                }
                else
                {
                    if (i != 9)
                    {
                        if (DisplayGraphics[i].Valid)
                            DisplayGraphics[i].Draw(pawnRect);
                    }
                }
            }
            //  for (int i = 0; i < 8; i++)
            //  {
            //      DisplayGraphics[i].Draw(pawnRect);
            //  }

            //    else
            //    {
            //        if (DisplayGraphics[6].Valid)
            //        {
            //            DisplayGraphics[6].Draw(pawnRect);
            //        }
            //    }
            //    DisplayGraphics[6].Draw(apparelRect);

            GUI.DrawTexture(new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height), _nameBackground);
            Widgets.Label(labelRect, nameStringShort);
            Widgets.DrawMenuSection(listRect);

            var set = new Rect(selectionRect);
            set.height = 30f;
            set.width = selectionRect.width / 2 - 10f;

            if (Widgets.ButtonText(set, "Hair"))
                Page = "hair";
            set.x += set.width + 10f;
            if (pawn.gender == Gender.Male)
            {
                if (Widgets.ButtonText(set, "Beard"))
                    Page = "beard";
            }
            if (Page == "hair")
            {
            set.x = selectionRect.x;
                set.y += 36f;
                set.width = selectionRect.width / 3 - 10f;
                if (Widgets.ButtonText(set, "Female"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    _hairDefs.SortBy(i => i.LabelCap);
                }
                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Male"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    _hairDefs.SortBy(i => i.LabelCap);
                }
                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Any"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairGender == HairGender.Any);
                    _hairDefs.SortBy(i => i.LabelCap);
                }
            }
            set.y += 36f;
            set.width = selectionRect.width / 2 - 10f;
            set.x = selectionRect.x;

            if (Widgets.ButtonText(set, "Eye"))
                Page = "eye";
            set.x += set.width + 10f;

            if (Widgets.ButtonText(set, "Brow"))
                Page = "brow";

            if (Page == "eye")
            {
                set.y += 36f;
                if (Widgets.ButtonText(set, "Female"))
                {
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    _eyeDefs.SortBy(i => i.LabelCap);
                }
                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Male"))
                {
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    _eyeDefs.SortBy(i => i.LabelCap);
                }
            }



            set.y += 36f;
            set.x = selectionRect.x;

            if (newBeard.drawMouth || pawn.gender == Gender.Female)
            {
                if (Widgets.ButtonText(set, "Mouth"))
                    Page = "mouth";
            }



            if (Page == "hair")
            {
                DrawHairPicker(listRect);
            }
            if (Page == "beard")
            {
                DrawBeardPicker(listRect);
            }
            if (Page == "mouth")
            {
                DrawMouthPicker(listRect);
            }
            if (Page == "eye")
            {
                DrawEyePicker(listRect);
            }
            if (Page == "brow")
            {
                DrawBrowPicker(listRect);
            }

            GUI.EndGroup();
        }

        private void DrawHairPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt((float)_hairDefs.Count / (float)_columns);


            rect3.height = (float)num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / (float)_columns;
                vector.y -= 16f / (float)_columns;
                rect3.width -= 16f;
                rect3.height = vector.y * ((float)num);
            }
            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref _scrollPosition, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < _hairDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect((float)num3 * vector.x, (float)num2 * vector.y, vector.x, vector.y);
                DrawHairPickerCell(_hairDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();

        }

        private void DrawBeardPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt((float)_beardDefs.Count / (float)_columns);

            rect3.height = (float)num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / (float)_columns;
                vector.y -= 16f / (float)_columns;
                rect3.width -= 16f;
                rect3.height = vector.y * ((float)num);
            }
            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref _scrollPosition, rect3);
            GUI.BeginGroup(rect3);


            for (int i = 0; i < _beardDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect((float)num3 * vector.x, (float)num2 * vector.y, vector.x, vector.y);
                DrawBeardPickerCell(_beardDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();

        }

        private void DrawMouthPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt((float)_lipDefs.Count / (float)_columns);

            rect3.height = (float)num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / (float)_columns;
                vector.y -= 16f / (float)_columns;
                rect3.width -= 16f;
                rect3.height = vector.y * ((float)num);
            }
            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref _scrollPosition, rect3);
            GUI.BeginGroup(rect3);


            for (int i = 0; i < _lipDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect((float)num3 * vector.x, (float)num2 * vector.y, vector.x, vector.y);
                DrawLipPickerCell(_lipDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();

        }

        private void DrawEyePicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt((float)_eyeDefs.Count / (float)_columns);

            rect3.height = (float)num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / (float)_columns;
                vector.y -= 16f / (float)_columns;
                rect3.width -= 16f;
                rect3.height = vector.y * ((float)num);
            }
            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref _scrollPosition, rect3);
            GUI.BeginGroup(rect3);


            for (int i = 0; i < _eyeDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect((float)num3 * vector.x, (float)num2 * vector.y, vector.x, vector.y);
                DrawEyePickerCell(_eyeDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();

        }

        private void DrawBrowPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt((float)_browDefs.Count / (float)_columns);

            rect3.height = (float)num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / (float)_columns;
                vector.y -= 16f / (float)_columns;
                rect3.width -= 16f;
                rect3.height = vector.y * ((float)num);
            }
            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref _scrollPosition, rect3);
            GUI.BeginGroup(rect3);


            for (int i = 0; i < _browDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect((float)num3 * vector.x, (float)num2 * vector.y, vector.x, vector.y);
                DrawBrowPickerCell(_browDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();

        }

        private void DrawHairPickerCell(HairDef hair, Rect rect)
        {
            GUI.DrawTexture(rect, HairGraphic(hair).MatFront.mainTexture);
            string text = hair.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (hair == newHair)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (hair == originalHair)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }
            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                newHair = hair;
                while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                {
                }
                Find.WindowStack.Add(new Dialog_ColorPicker(colourWrapper, delegate
                {
                    newColour = colourWrapper.Color;
                }, false, true)
                {
                    initialPosition = new Vector2(windowRect.xMax + _margin, windowRect.yMin),
                });
            }
        }

        private void DrawBeardPickerCell(BeardDef beard, Rect rect)
        {
            GUI.DrawTexture(rect, BeardGraphic(beard).MatFront.mainTexture);
            string text = beard.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (beard == newBeard)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (beard == originalBeard)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }
            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                newBeard = beard;
                while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                {
                }
                Find.WindowStack.Add(new Dialog_ColorPicker(colourWrapper, delegate
                {
                    newColour = colourWrapper.Color;
                }, false, true)
                {
                    initialPosition = new Vector2(windowRect.xMax + _margin, windowRect.yMin),
                });
            }
        }

        private void DrawLipPickerCell(LipDef lip, Rect rect)
        {
            GUI.DrawTexture(rect, LipGraphic(lip).MatFront.mainTexture);
            string text = lip.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (lip == newLip)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (lip == originalLip)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }
            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                newLip = lip;
                Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker));
            }
        }

        private void DrawEyePickerCell(EyeDef eye, Rect rect)
        {
            GUI.DrawTexture(rect, EyeGraphic(eye).MatFront.mainTexture);
            string text = eye.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (eye == newEye)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (eye == originalEye)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }
            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                newEye = eye;
                Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker));

            }
        }

        private void DrawBrowPickerCell(BrowDef brow, Rect rect)
        {
            GUI.DrawTexture(rect, BrowGraphic(brow).MatFront.mainTexture);
            string text = brow.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (brow == newBrow)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (brow == originalBrow)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }
            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                newBrow = brow;
                Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker));

            }
        }


        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(_iconSize + _margin, 0f, inRect.width - _iconSize - _margin, _titleHeight);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, _title);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            Rect position = new Rect(0f, 0f, _iconSize, _iconSize).CenteredOnYIn(rect);
            GUI.DrawTexture(position, _icon);
            DrawUI(new Rect(0f, _titleHeight + _margin, inRect.width, inRect.height - _titleHeight - 38f - _margin));
            DialogUtility.DoNextBackButtons(inRect, "ClutterColorChangerButtonAccept".Translate(), delegate
            {
                // update render for graphics
                pawn.Drawer.renderer.graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(newHair.texPath, ShaderDatabase.Cutout, Vector2.one, newColour);

                // update story to persist across save/load
                pawn.story.hairColor = newColour;
                pawn.story.hairDef = newHair;
                var pawnSave = MapComponent_FacialStuff.GetCache(pawn);
                if (pawn.gender == Gender.Male)
                {
                    pawnSave.BeardDef = newBeard;
                }
                pawnSave.LipDef = newLip;
                pawnSave.sessionOptimized = false;
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();

                // force colonist bar to update
                PortraitsCache.SetDirty(pawn);
                Close();
            }, delegate
            {
                Close();
            });
        }
    }
}
