namespace FaceStyling
{
    using System.Collections.Generic;

    using FaceStyling.ColorPicker;

    using RimWorld;

    using RW_FacialStuff;
    using RW_FacialStuff.Defs;
    using RW_FacialStuff.Detouring;
    using RW_FacialStuff.Utilities;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public class Dialog_FaceStyling : Window
    {
        private static readonly List<BeardDef> _beardDefs;

        private static readonly int _columns;

        private static readonly float _entrySize;

        private static readonly float _iconSize;

        private static readonly float _listWidth;

        // private static Texture2D _icon;
        private static readonly float _margin;

        private static readonly List<MouthDef> _mouthDefs;

        private static readonly Texture2D _nameBackground;

        private static readonly float _previewSize;

        private static readonly string _title;

        private static readonly float _titleHeight;

        private static List<BrowDef> _browDefs;

        private static List<EyeDef> _eyeDefs;

        private static List<HairDef> _hairDefs;

        private static BeardDef _newBeard;

        private static BrowDef _newBrow;

        private static Color _newColour;

        private static EyeDef _newEye;

        private static HairDef _newHair;

        private static MouthDef _newMouth;

        private static ColorWrapper colourWrapper;

        private static GraphicsDisp[] DisplayGraphics;

        private static CompFace faceComp;

        private static BeardDef originalBeard;

        private static BrowDef originalBrow;

        private static Color originalColour;

        private static EyeDef originalEye;

        private static HairDef originalHair;

        private static MouthDef originalMouth;

        private static Pawn pawn;

        public string Page = "hair";

        private readonly float originalMelanin;

        private float _newMelanin;

        private Vector2 _scrollPosition = Vector2.zero;

        static Dialog_FaceStyling()
        {
            _title = "FaceStylerTitle".Translate();
            _titleHeight = 30f;
            _previewSize = 250f;

            // _previewSize = 100f;
            _iconSize = 24f;

            // _icon = ContentFinder<Texture2D>.Get("ClothIcon");
            _margin = 6f;
            _listWidth = 450f;

            // _listWidth = 200f;
            _columns = 5;
            _entrySize = _listWidth / _columns;
            _nameBackground = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));

            _hairDefs = DefDatabase<HairDef>.AllDefsListForReading;
            _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading;
            _beardDefs = DefDatabase<BeardDef>.AllDefsListForReading;
            _mouthDefs = DefDatabase<MouthDef>.AllDefsListForReading;
            _browDefs = DefDatabase<BrowDef>.AllDefsListForReading;
            _beardDefs.SortBy(i => i.LabelCap);
        }

        public Dialog_FaceStyling(Pawn p)
        {
            pawn = p;
            faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp.BeardDef == null)
            {
                faceComp.BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
            }

            this.absorbInputAroundWindow = false;
            this.forcePause = true;
            this.closeOnClickedOutside = false;
            originalColour = _newColour = faceComp.HairColorOrg;

            colourWrapper = new ColorWrapper(this.NewColour);

            this._newMelanin = this.originalMelanin = pawn.story.melanin;

            _newHair = originalHair = pawn.story.hairDef;
            _newBeard = originalBeard = faceComp.BeardDef;
            _newMouth = originalMouth = faceComp.MouthDef;
            _newEye = originalEye = faceComp.EyeDef;
            _newBrow = originalBrow = faceComp.BrowDef;

            DisplayGraphics = new GraphicsDisp[12];
            for (int i = 0; i < 12; i++)
            {
                DisplayGraphics[i] = new GraphicsDisp();
            }

            this.SetGraphicSlot(
                GraphicSlotGroup.Body,
                pawn,
                GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                    pawn.story.bodyType,
                    ShaderDatabase.CutoutSkin,
                    _PawnSkinColors.GetSkinColor(this.NewMelanin)),
                pawn.def.uiIcon,
                _PawnSkinColors.GetSkinColor(this.NewMelanin));
            this.SetGraphicSlot(
                GraphicSlotGroup.Head,
                pawn,
                GraphicDatabaseHeadRecords.GetHeadNamed(
                    pawn.story.HeadGraphicPath,
                    _PawnSkinColors.GetSkinColor(this.NewMelanin)),
                pawn.def.uiIcon,
                _PawnSkinColors.GetSkinColor(this.NewMelanin));
            this.SetGraphicSlot(
                GraphicSlotGroup.Hair,
                pawn,
                this.HairGraphic(pawn.story.hairDef),
                pawn.def.uiIcon,
                pawn.story.hairColor);
            this.SetGraphicSlot(
                GraphicSlotGroup.Eyes,
                pawn,
                this.EyeGraphic(faceComp.EyeDef),
                pawn.def.uiIcon,
                Color.black);
            this.SetGraphicSlot(
                GraphicSlotGroup.Brows,
                pawn,
                this.BrowGraphic(faceComp.BrowDef),
                pawn.def.uiIcon,
                pawn.story.hairColor * new Color(0.3f, 0.3f, 0.3f));
            this.SetGraphicSlot(
                GraphicSlotGroup.Mouth,
                pawn,
                this.MouthGraphic(faceComp.MouthDef),
                pawn.def.uiIcon,
                Color.black);
            this.SetGraphicSlot(
                GraphicSlotGroup.Beard,
                pawn,
                this.BeardGraphic(faceComp.BeardDef),
                pawn.def.uiIcon,
                pawn.story.hairColor);
            foreach (Apparel current in pawn.apparel.WornApparel)
            {
                GraphicSlotGroup slotForApparel = this.GetSlotForApparel(current);
                if (slotForApparel != GraphicSlotGroup.Invalid)
                {
                    if (current.def.apparel.LastLayer != ApparelLayer.Overhead)
                    {
                        this.SetGraphicSlot(
                            slotForApparel,
                            current,
                            this.ApparelGraphic(current.def, pawn.story.bodyType),
                            current.def.uiIcon,
                            current.DrawColor);
                    }
                }
            }
        }

        private enum GraphicSlotGroup
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

            Mouth,

            Beard,

            Hair,

            Overhead
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(
                    _previewSize + _margin + _listWidth + 36f,
                    40f + _previewSize * 2f + _margin * 3f + 38f + 36f + 80f);
            }
        }

        public Color NewColour
        {
            get
            {
                return _newColour;
            }

            set
            {
                _newColour = value;
                DisplayGraphics[7].color = value;
                DisplayGraphics[9].color = value;
                DisplayGraphics[10].color = value;
            }
        }

        public BeardDef NewBeard
        {
            get
            {
                return _newBeard;
            }

            set
            {
                _newBeard = value;
                this.SetGraphicSlot(
                    GraphicSlotGroup.Beard,
                    pawn,
                    this.BeardGraphic(value),
                    pawn.def.uiIcon,
                    this.NewColour);
            }
        }

        public BrowDef NewBrow
        {
            get
            {
                return _newBrow;
            }

            set
            {
                _newBrow = value;
                this.SetGraphicSlot(
                    GraphicSlotGroup.Brows,
                    pawn,
                    this.BrowGraphic(value),
                    pawn.def.uiIcon,
                    this.NewColour * new Color(0.3f, 0.3f, 0.3f));
            }
        }

        public EyeDef NewEye
        {
            get
            {
                return _newEye;
            }

            set
            {
                _newEye = value;
                this.SetGraphicSlot(
                    GraphicSlotGroup.Eyes,
                    pawn,
                    this.EyeGraphic(value),
                    pawn.def.uiIcon,
                    this.NewColour);
            }
        }

        public HairDef NewHair
        {
            get
            {
                return _newHair;
            }

            set
            {
                _newHair = value;
                this.SetGraphicSlot(
                    GraphicSlotGroup.Hair,
                    pawn,
                    this.HairGraphic(value),
                    pawn.def.uiIcon,
                    this.NewColour);
            }
        }

        public float NewMelanin
        {
            get
            {
                return this._newMelanin;
            }

            set
            {
                this._newMelanin = value;
                this.SetGraphicSlot(
                    GraphicSlotGroup.Body,
                    pawn,
                    GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                        pawn.story.bodyType,
                        ShaderDatabase.CutoutSkin,
                        _PawnSkinColors.GetSkinColor(this._newMelanin)),
                    pawn.def.uiIcon,
                    _PawnSkinColors.GetSkinColor(this._newMelanin));

                this.SetGraphicSlot(
                    GraphicSlotGroup.Head,
                    pawn,
                    GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(
                        pawn,
                        true,
                        _PawnSkinColors.GetSkinColor(this._newMelanin)),
                    pawn.def.uiIcon,
                    _PawnSkinColors.GetSkinColor(this._newMelanin));
            }
        }

        public MouthDef NewMouth
        {
            get
            {
                return _newMouth;
            }

            set
            {
                _newMouth = value;
                this.SetGraphicSlot(
                    GraphicSlotGroup.Mouth,
                    pawn,
                    this.MouthGraphic(value),
                    pawn.def.uiIcon,
                    this.NewColour);
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

            // Rect iconPosition = new Rect(0f, 0f, _iconSize, _iconSize).CenteredOnYIn(rect);
            // GUI.DrawTexture(iconPosition, _icon);
            this.DrawUI(new Rect(0f, _titleHeight, inRect.width, inRect.height - _titleHeight - 25f - _margin * 2));
            DialogUtility.DoNextBackButtons(
                inRect,
                "FacialStuffColorChangerButtonAccept".Translate(),
                delegate
                    {
                        // update render for graphics
                        pawn.Drawer.renderer.graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(
                            this.NewHair.texPath,
                            ShaderDatabase.Cutout,
                            Vector2.one,
                            this.NewColour);

                        // update story to persist across save/load
                        pawn.story.hairColor = this.NewColour;
                        pawn.story.hairDef = this.NewHair;
                        pawn.story.melanin = this.NewMelanin;

                        // FS additions
                        if (pawn.gender == Gender.Male)
                        {
                            faceComp.BeardDef = this.NewBeard;
                        }

                        faceComp.EyeDef = this.NewEye;
                        faceComp.BrowDef = this.NewBrow;
                        faceComp.MouthDef = this.NewMouth;
                        faceComp.sessionOptimized = false;
                        pawn.Drawer.renderer.graphics.ResolveAllGraphics();


                        this.Close();
                    },
                delegate { this.Close(); });
        }

        public override void PostOpen()
        {
            switch (pawn.gender)
            {
                case Gender.Male:
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    _browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    break;
                case Gender.Female:
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    _browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    break;
            }
            _hairDefs.SortBy(i => i.LabelCap);
            _eyeDefs.SortBy(i => i.LabelCap);
            _browDefs.SortBy(i => i.LabelCap);
        }

        public override void PreClose()
        {
            while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker), false))
            {
            }
        }

        private Graphic ApparelGraphic(ThingDef def, BodyType bodyType)
        {
            Graphic result;
            if (def.apparel == null || def.apparel.wornGraphicPath.NullOrEmpty())
            {
                result = null;
            }
            else
            {
                result = GraphicDatabase.Get<Graphic_Multi>(
                    (def.apparel.LastLayer != ApparelLayer.Overhead)
                        ? (def.apparel.wornGraphicPath + "_" + bodyType)
                        : def.apparel.wornGraphicPath,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white);
            }

            return result;
        }

        private Graphic BeardGraphic(BeardDef def)
        {
            Graphic result;
            if (def.texPathAverageNormal != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    def.texPathAverageNormal,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white);
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
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    def.texPath,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white);
            }
            else
            {
                result = null;
            }

            return result;
        }

        private void DrawBeardPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(_beardDefs.Count / (float)_columns);

            rect3.height = num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / _columns;
                vector.y -= 16f / _columns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this._scrollPosition, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < _beardDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawBeardPickerCell(_beardDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawBeardPickerCell(BeardDef beard, Rect rect)
        {
            GUI.DrawTexture(rect, this.BeardGraphic(beard).MatFront.mainTexture);
            string text = beard.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (beard == this.NewBeard)
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
                this.NewBeard = beard;
                while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                {
                }

                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        colourWrapper,
                        delegate { this.NewColour = colourWrapper.Color; },
                        false,
                        true) {
                                 initialPosition = new Vector2(this.windowRect.xMax + _margin, this.windowRect.yMin) 
                              });
            }
        }

        private void DrawBrowPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(_browDefs.Count / (float)_columns);

            rect3.height = num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / _columns;
                vector.y -= 16f / _columns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this._scrollPosition, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < _browDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawBrowPickerCell(_browDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawBrowPickerCell(BrowDef brow, Rect rect)
        {
            GUI.DrawTexture(rect, this.BrowGraphic(brow).MatFront.mainTexture);
            string text = brow.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (brow == this.NewBrow)
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
                this.NewBrow = brow;
                Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker));
            }
        }

        private void DrawColorPickerCell(Color color, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, color);
            string text = color.ToString();
            Widgets.DrawHighlightIfMouseover(rect);
            if (color == this.NewColour)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (color == originalColour)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                {
                }

                colourWrapper.Color = color;
                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        colourWrapper,
                        delegate { this.NewColour = colourWrapper.Color; },
                        false,
                        true) {
                                 initialPosition = new Vector2(this.windowRect.xMax + _margin, this.windowRect.yMin) 
                              });
            }
        }

        private void DrawEyePicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(_eyeDefs.Count / (float)_columns);

            rect3.height = num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / _columns;
                vector.y -= 16f / _columns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this._scrollPosition, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < _eyeDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawEyePickerCell(_eyeDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawEyePickerCell(EyeDef eye, Rect rect)
        {
            GUI.DrawTexture(rect, this.EyeGraphic(eye).MatFront.mainTexture);
            string text = eye.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (eye == this.NewEye)
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
                this.NewEye = eye;
                Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker));
            }
        }

        private void DrawHairPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(_hairDefs.Count / (float)_columns);

            rect3.height = num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / _columns;
                vector.y -= 16f / _columns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this._scrollPosition, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < _hairDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawHairPickerCell(_hairDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawHairPickerCell(HairDef hair, Rect rect)
        {
            GUI.DrawTexture(rect, this.HairGraphic(hair).MatFront.mainTexture);
            string text = hair.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (hair == this.NewHair)
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

            Widgets.Label(rect, text);
            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewHair = hair;
                while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                {
                }

                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        colourWrapper,
                        delegate { this.NewColour = colourWrapper.Color; },
                        false,
                        true) {
                                 initialPosition = new Vector2(this.windowRect.xMax + _margin, this.windowRect.yMin) 
                              });
            }
        }

        private void DrawMelaninPickerCell(_PawnSkinColors.SkinColorData data, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, data.color);
            string text = data.ToString();
            Widgets.DrawHighlightIfMouseover(rect);
            if (data.melanin == this.NewMelanin)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (data.melanin == this.originalMelanin)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewMelanin = data.melanin;
            }
        }

        private void DrawMouthPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(_mouthDefs.Count / (float)_columns);

            rect3.height = num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / _columns;
                vector.y -= 16f / _columns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this._scrollPosition, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < _mouthDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawMouthPickerCell(_mouthDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawMouthPickerCell(MouthDef mouth, Rect rect)
        {
            GUI.DrawTexture(rect, this.MouthGraphic(mouth).MatFront.mainTexture);
            string text = mouth.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (mouth == this.NewMouth)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (mouth == originalMouth)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewMouth = mouth;
                Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker));
            }
        }

        private void DrawUI(Rect parentRect)
        {
            GUI.BeginGroup(parentRect);
            string nameStringShort = pawn.NameStringShort;
            Vector2 vector = Text.CalcSize(nameStringShort);
            Rect pawnHeadRect = new Rect(0f, -10f, _previewSize, _previewSize);
            Rect pawnRect = new Rect(0f, 0f, _previewSize, _previewSize);
            Rect labelRect = new Rect(0f, pawnRect.yMax - vector.y, vector.x, vector.y);
            Rect melaninRect = new Rect(0f, labelRect.yMax + _margin, _previewSize, 30f);
            Rect selectionRect = new Rect(0f, melaninRect.yMax + _margin, _previewSize, _previewSize);
            Rect listRect = new Rect(_previewSize + _margin, 0f, _listWidth, parentRect.height - _margin * 2);

            labelRect = labelRect.CenteredOnXIn(pawnRect);
            for (int i = 0; i < DisplayGraphics.Length; i++)
            {
                if (pawn.gender == Gender.Male)
                {
                    if (!this.NewBeard.drawMouth || !faceComp.drawMouth)
                    {
                        // layer 1-5 = body
                        if (i <= 5)
                        {
                            if (DisplayGraphics[i].Valid)
                            {
                                DisplayGraphics[i].Draw(pawnRect);
                            }
                        }
                        else
                        {
                            if (i != 8)
                            {
                                if (DisplayGraphics[i].Valid)
                                {
                                    DisplayGraphics[i].Draw(pawnHeadRect);
                                }
                            }
                        }
                    }
                    else if (DisplayGraphics[i].Valid)
                    {
                        if (i <= 5)
                        {
                            DisplayGraphics[i].Draw(pawnRect);
                        }
                        else
                        {
                            DisplayGraphics[i].Draw(pawnHeadRect);
                        }
                    }
                }
                else
                {
                    if (i <= 5)
                    {
                        if (DisplayGraphics[i].Valid)
                        {
                            DisplayGraphics[i].Draw(pawnRect);
                        }
                    }
                    else if (i != 9)
                    {
                        if (!faceComp.drawMouth)
                        {
                            if (i != 8)
                            {
                                if (DisplayGraphics[i].Valid)
                                {
                                    DisplayGraphics[i].Draw(pawnHeadRect);
                                }
                            }
                        }
                        else if (DisplayGraphics[i].Valid)
                        {
                            DisplayGraphics[i].Draw(pawnHeadRect);
                        }
                    }
                }
            }

            // for (int i = 0; i < 8; i++)
            // {
            // DisplayGraphics[i].Draw(pawnRect);
            // }

            // else
            // {
            // if (DisplayGraphics[6].Valid)
            // {
            // DisplayGraphics[6].Draw(pawnRect);
            // }
            // }
            // DisplayGraphics[6].Draw(apparelRect);
            GUI.DrawTexture(
                new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height),
                _nameBackground);
            Widgets.Label(labelRect, nameStringShort);
            float width = _previewSize / _PawnSkinColors._SkinColors.Length - 10f;

            // float spacing = 10f;
            Rect setMelanin = new Rect(0f, melaninRect.y, width, melaninRect.height);
            foreach (var skinColorData in _PawnSkinColors._SkinColors)
            {
                this.DrawMelaninPickerCell(skinColorData, setMelanin);
                setMelanin.x += setMelanin.width + 10f;
            }

            Widgets.DrawMenuSection(listRect);

            Rect set = new Rect(selectionRect) { height = 30f, width = selectionRect.width / 2 - 10f };

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
            if (Widgets.ButtonText(set, "Hair"))
            {
                this.Page = "hair";
            }

            set.x += set.width + 10f;
            if (pawn.gender == Gender.Male)
            {
                if (Widgets.ButtonText(set, "Beard"))
                {
                    this.Page = "beard";
                }
            }

            set.y += 36f;
            set.width = selectionRect.width / 2 - 10f;
            set.x = selectionRect.x;

            if (Widgets.ButtonText(set, "Eye"))
            {
                this.Page = "eye";
            }

            set.x += set.width + 10f;

            if (Widgets.ButtonText(set, "Brow"))
            {
                this.Page = "brow";
            }

            set.y += 36f;
            set.x = selectionRect.x;
            set.width = selectionRect.width;
            Widgets.CheckboxLabeled(set, "Draw colonist mouth if suitable", ref faceComp.drawMouth);
            set.width = selectionRect.width / 2 - 10f;

            set.y += 36f;
            set.x = selectionRect.x;

            if (this.NewBeard.drawMouth || pawn.gender == Gender.Female)
            {
                if (Widgets.ButtonText(set, "Mouth"))
                {
                    this.Page = "mouth";
                }
            }

            if (this.Page == "hair")
            {
                set.x = selectionRect.x;
                set.y += 36f;
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(selectionRect.x, set.y, selectionRect.width);
                GUI.color = Color.white;
                set.y += 12f;
                set.width = selectionRect.width / 3 - 10f;
                if (Widgets.ButtonText(set, "Female"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    _hairDefs.SortBy(i => i.LabelCap);
                }

                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Male"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    _hairDefs.SortBy(i => i.LabelCap);
                }

                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Any"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairGender == HairGender.Any);
                    _hairDefs.SortBy(i => i.LabelCap);
                }

                this.DrawHairPicker(listRect);
            }

            if (this.Page == "eye")
            {
                set.x = selectionRect.x;
                set.y += 36f;
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(selectionRect.x, set.y, selectionRect.width);
                GUI.color = Color.white;
                set.y += 12f;

                if (Widgets.ButtonText(set, "Female"))
                {
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    _eyeDefs.SortBy(i => i.LabelCap);
                }

                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Male"))
                {
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    _eyeDefs.SortBy(i => i.LabelCap);
                }
            }

            if (this.Page == "brow")
            {
                set.x = selectionRect.x;
                set.y += 36f;
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(selectionRect.x, set.y, selectionRect.width);
                GUI.color = Color.white;
                set.y += 12f;

                if (Widgets.ButtonText(set, "Female"))
                {
                    _browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    _browDefs.SortBy(i => i.LabelCap);
                }

                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Male"))
                {
                    _browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    _browDefs.SortBy(i => i.LabelCap);
                }

                this.DrawBrowPicker(listRect);
            }

            if (this.Page == "beard")
            {
                this.DrawBeardPicker(listRect);
            }

            if (this.Page == "mouth")
            {
                this.DrawMouthPicker(listRect);
            }

            if (this.Page == "eye")
            {
                this.DrawEyePicker(listRect);
            }

            if (this.Page == "hair" || this.Page == "beard")
            {
                set.y += 36f;
                set.width = selectionRect.width / 7.5f - 10f;
                set.x = selectionRect.x;

                this.DrawColorPickerCell(originalColour, set);
                set.x += set.width * 1.5f + 10f;

                this.DrawColorPickerCell(_PawnHairColors.HairPlatinum, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairYellowBlonde, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairTerraCotta, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairMediumDarkBrown, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairDarkBrown, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairMidnightBlack, set);
                set.x += set.width + 10f;

                set.y += 36f;
                set.x = selectionRect.x;
                set.width = selectionRect.width / 10f - 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairDarkPurple, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairBlueSteel, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairBurgundyBistro, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairGreenGrape, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairMysticTurquois, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairPinkPearl, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairPurplePassion, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairRosaRosa, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairRubyRed, set);
                set.x += set.width + 10f;
                this.DrawColorPickerCell(_PawnHairColors.HairUltraViolet, set);
                set.x += set.width + 10f;
            }

            GUI.EndGroup();
        }

        private Graphic EyeGraphic(EyeDef def)
        {
            Graphic result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    def.texPath,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white);
            }
            else
            {
                result = null;
            }

            return result;
        }

        private GraphicSlotGroup GetSlotForApparel(Thing apparel)
        {
            ApparelProperties apparel2 = apparel.def.apparel;
            ApparelLayer lastLayer = apparel2.LastLayer;
            GraphicSlotGroup result;
            switch (lastLayer)
            {
                case ApparelLayer.OnSkin:
                    if (apparel2.bodyPartGroups.Count == 1
                        && apparel2.bodyPartGroups[0].Equals(BodyPartGroupDefOf.Legs))
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
            Log.Warning("Could not resolve 'LastLayer' " + lastLayer);
            result = GraphicSlotGroup.Invalid;
            return result;
        }

        private Graphic HairGraphic(HairDef def)
        {
            Graphic result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi>(
                    def.texPath,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white);
            }
            else
            {
                result = null;
            }

            return result;
        }

        private Graphic MouthGraphic(MouthDef def)
        {
            Graphic result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_HeadParts>(
                    def.texPath,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white);
            }
            else
            {
                result = null;
            }

            return result;
        }

        private void SetGraphicSlot(
            GraphicSlotGroup slotIndex,
            Thing newThing,
            Graphic newGraphic,
            Texture2D newIcon,
            Color newColor)
        {
            if (slotIndex < GraphicSlotGroup.Head)
            {
                DisplayGraphics[(int)slotIndex] = new GraphicsDisp(newThing, newGraphic, newIcon, newColor, 0f);
            }

            if (slotIndex == GraphicSlotGroup.Head)
            {
                DisplayGraphics[(int)slotIndex] = new GraphicsDisp(newThing, newGraphic, newIcon, newColor, -50f);
            }

            if (slotIndex > GraphicSlotGroup.Head)
            {
                DisplayGraphics[(int)slotIndex] = new GraphicsDisp(newThing, newGraphic, newIcon, newColor, -40f);
            }
        }

        // private FacePreset _selFacePresetInt;

        // private FacePreset SelectedFacePreset
        // {
        // get { return _selFacePresetInt; }
        // set
        // {
        // CheckSelectedFacePresetHasName();
        // _selFacePresetInt = value;
        // }
        // }

        // private void CheckSelectedFacePresetHasName()
        // {
        // if (SelectedFacePreset != null && SelectedFacePreset.label.NullOrEmpty())
        // {
        // SelectedFacePreset.label = "Unnamed";
        // }
        // }
        public class GraphicsDisp
        {
            public Color color = Color.white;

            public Graphic graphic;

            public Texture2D icon;

            public Thing thing;

            private readonly float yOffset;

            public GraphicsDisp()
            {
                this.thing = null;
                this.graphic = null;
                this.icon = null;
                this.yOffset = 0f;
            }

            public GraphicsDisp(Thing thing, Graphic graphic, Texture2D icon, Color color, float yOffset)
            {
                this.thing = thing;
                this.graphic = graphic;
                this.icon = icon;
                this.color = color;
                this.yOffset = yOffset;
            }

            public bool Valid
            {
                get
                {
                    return this.thing != null;
                }
            }

            public void Draw(Rect rect)
            {
                if (this.Valid && this.graphic != null)
                {
                    Rect position = new Rect(rect.x, rect.y + this.yOffset, rect.width, rect.height);
                    GUI.color = this.color;
                    GUI.DrawTexture(position, this.graphic.MatFront.mainTexture);
                    GUI.color = Color.white;
                }
            }

            public void DrawIcon(Rect rect)
            {
                if (this.Valid && this.icon != null)
                {
                    GUI.color = this.color;
                    GUI.DrawTexture(rect, this.icon);
                    GUI.color = Color.white;
                }
            }
        }
    }
}