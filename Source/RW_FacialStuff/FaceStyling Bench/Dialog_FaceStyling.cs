namespace FacialStuff.FaceStyling_Bench
{
    using System.Collections.Generic;

    using FaceStyling;

    using FacialStuff;
    using FacialStuff.ColorPicker;
    using FacialStuff.Defs;
    using FacialStuff.Detouring;
    using FacialStuff.Utilities;

    using RimWorld;

    using UnityEngine;

    using Verse;

    // bug: update for 2nd eye display, make beard color selectable, 
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

        private static readonly Texture2D _nameBackground;

        private static readonly float _previewSize;

        private static readonly string _title;

        private static readonly float _titleHeight;

        private static List<BrowDef> _browDefs;

        private static List<EyeDef> _eyeDefs;

        private static List<HairDef> _hairDefs;

        private static BeardDef _newBeard;

        private static BrowDef _newBrow;

        private static Color _newHairColour;

        private static Color _newBeardColour;

        private static EyeDef _newEye;

        private static HairDef _newHair;

        private static ColorWrapper colourWrapper;

        private static GraphicsDisp[] DisplayGraphics;

        private static CompFace faceComp;

        private static BeardDef originalBeard;

        private static BrowDef originalBrow;

        private static Color originalColour;

        private static EyeDef originalEye;

        private static HairDef originalHair;

        private static Pawn pawn;

        public string Page = "hair";

        private readonly float originalMelanin;

        private float _newMelanin;

        private Vector2 _scrollPosition = Vector2.zero;

        private Graphic_Multi_NaturalEyes RightEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes result;
            if (def.texPath != null)
            {
                string path = faceComp.EyeTexPath(def.texPath, enums.Side.Right);

                // "Eyes/Eye_" + pawn.gender + faceComp.crownType + "_" + def.texPath   + "_Right";
                result = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                             path,
                             ShaderDatabase.Cutout,
                             new Vector2(38f, 38f),
                             Color.white,
                             Color.white) as Graphic_Multi_NaturalEyes;
            }
            else
            {
                result = null;
            }

            return result;
        }

        private Graphic_Multi_NaturalEyes LeftEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes result;
            if (def.texPath != null)
            {
                string path = faceComp.EyeTexPath(def.texPath, enums.Side.Left);


                result = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                             path,
                             ShaderDatabase.Cutout,
                             new Vector2(38f, 38f),
                             Color.white,
                             Color.white) as Graphic_Multi_NaturalEyes;
            }
            else
            {
                result = null;
            }

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

        private Graphic_Multi_NaturalHeadParts BeardGraphic(BeardDef def)
        {
            Graphic_Multi_NaturalHeadParts result;
            if (def.texPath != null)
            {
                string path = def.texPath + "_" + faceComp.crownType + "_" + faceComp.headType;
                if (def == DefDatabase<BeardDef>.GetNamed("Beard_Shaved"))
                {
                    path = def.texPath;
                }

                result = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    path,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_Multi_NaturalHeadParts;
            }
            else
            {
                result = null;
            }

            return result;
        }

        private Graphic_Multi_NaturalHeadParts BrowGraphic(BrowDef def)
        {
            Graphic_Multi_NaturalHeadParts result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    faceComp.BrowTexPath(def),
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_Multi_NaturalHeadParts;
            }
            else
            {
                result = null;
            }

            return result;
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
            _browDefs = DefDatabase<BrowDef>.AllDefsListForReading;
            _beardDefs.SortBy(i => i.LabelCap);
        }

        public Dialog_FaceStyling(Pawn p)
        {
            pawn = p;
            faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp != null)
            {
                if (faceComp.BeardDef == null)
                {
                    faceComp.BeardDef = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
                }

                originalColour = faceComp.HairColorOrg;
                _newBeardColour = faceComp.BeardColor;
                _newBeard = originalBeard = faceComp.BeardDef;
                _newEye = originalEye = faceComp.EyeDef;
                _newBrow = originalBrow = faceComp.BrowDef;
            }
            else
            {
                originalColour = pawn.story.hairColor;
            }

            this.absorbInputAroundWindow = false;
            this.forcePause = true;
            this.closeOnClickedOutside = false;

            _newHairColour = pawn.story.hairColor;

            colourWrapper = new ColorWrapper(Color.cyan);

            this._newMelanin = this.originalMelanin = pawn.story.melanin;

            _newHair = originalHair = pawn.story.hairDef;


            DisplayGraphics = new GraphicsDisp[(int)enums.GraphicSlotGroup.NumberOfTypes];
            for (int i = 0; i < (int)enums.GraphicSlotGroup.NumberOfTypes; i++)
            {
                DisplayGraphics[i] = new GraphicsDisp();
            }


            this.SetGraphicSlot(
                enums.GraphicSlotGroup.Body,
                pawn,
                GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                    pawn.story.bodyType,
                    ShaderDatabase.CutoutSkin,
                    pawn.story.SkinColor),
                pawn.def.uiIcon,
                pawn.story.SkinColor);

            if (faceComp != null)
            {
                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.Head,
                    pawn,
                    GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(
                        pawn,
                        pawn.story.SkinColor),
                    pawn.def.uiIcon,
                    pawn.story.SkinColor);
            }
            else
            {
                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.Head,
                    pawn,
                   pawn.Drawer.renderer.graphics.headGraphic,
                    pawn.def.uiIcon,
                    pawn.story.SkinColor);
            }

            this.SetGraphicSlot(
                enums.GraphicSlotGroup.Hair,
                pawn,
                pawn.Drawer.renderer.graphics.hairGraphic,
                pawn.def.uiIcon,
                pawn.story.hairColor);

            if (faceComp != null)
            {

                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.RightEye,
                    pawn,
                    this.RightEyeGraphic(faceComp.EyeDef),
                    pawn.def.uiIcon,
                    Color.black);

                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.LeftEye,
                    pawn,
                    this.LeftEyeGraphic(faceComp.EyeDef),
                    pawn.def.uiIcon,
                    Color.black);

                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.Brows,
                    pawn,
                    this.BrowGraphic(faceComp.BrowDef),
                    pawn.def.uiIcon,
                    Color.black);

                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.Mouth,
                    pawn,
                    faceComp.MouthGraphic,
                    pawn.def.uiIcon,
                   faceComp.HasNaturalMouth ? Color.black : Color.white);

                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.Beard,
                    pawn,
                    this.BeardGraphic(faceComp.BeardDef),
                    pawn.def.uiIcon,
                    faceComp.BeardColor);
            }

            foreach (Apparel current in pawn.apparel.WornApparel)
            {
                enums.GraphicSlotGroup slotForApparel = this.GetSlotForApparel(current);
                if (slotForApparel != enums.GraphicSlotGroup.Invalid)
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

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(
                    _previewSize + _margin + _listWidth + 36f,
                    40f + _previewSize * 2f + _margin * 3f + 38f + 36f + 80f);
            }
        }

        public Color NewHairColour
        {
            get
            {
                return _newHairColour;
            }

            set
            {
                _newHairColour = value;
                DisplayGraphics[(int)enums.GraphicSlotGroup.Hair].color = value;
                if (faceComp != null && faceComp.HasSameBeardColor)
                {
                    DisplayGraphics[(int)enums.GraphicSlotGroup.Beard].color = PawnHairColors_PostFix.DarkerBeardColor(value);
                }
            }
        }

        public Color NewBeardColour
        {
            get
            {
                return _newBeardColour;
            }

            set
            {
                _newBeardColour = value;
                DisplayGraphics[(int)enums.GraphicSlotGroup.Beard].color = value;
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
                    enums.GraphicSlotGroup.Beard,
                    pawn,
                    this.BeardGraphic(value),
                    pawn.def.uiIcon,
                    this.NewBeardColour);
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
                    enums.GraphicSlotGroup.Brows,
                    pawn,
                    this.BrowGraphic(value),
                    pawn.def.uiIcon,
                    Color.black);
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
                    enums.GraphicSlotGroup.RightEye,
                    pawn,
                    this.RightEyeGraphic(value),
                    pawn.def.uiIcon,
                    Color.black);

                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.LeftEye,
                    pawn,
                    this.LeftEyeGraphic(value),
                    pawn.def.uiIcon,
                    Color.black);
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
                    enums.GraphicSlotGroup.Hair,
                    pawn,
                    this.HairGraphic(value),
                    pawn.def.uiIcon,
                    this.NewHairColour);
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
                    enums.GraphicSlotGroup.Body,
                    pawn,
                    GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                        pawn.story.bodyType,
                        ShaderDatabase.CutoutSkin,
                        PawnSkinColors.GetSkinColor(this._newMelanin)),
                    pawn.def.uiIcon,
                    PawnSkinColors.GetSkinColor(this._newMelanin));

                this.SetGraphicSlot(
                    enums.GraphicSlotGroup.Head,
                    pawn,
                    GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(
                        pawn,
                        PawnSkinColors.GetSkinColor(this._newMelanin)),
                    pawn.def.uiIcon,
                    PawnSkinColors.GetSkinColor(this._newMelanin));
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
                            this.NewHairColour);

                        // update story to persist across save/load
                        pawn.story.hairColor = this.NewHairColour;
                        pawn.story.hairDef = this.NewHair;
                        pawn.story.melanin = this.NewMelanin;
                        faceComp.MelaninOrg = this.NewMelanin;

                        if (faceComp != null)
                        {

                            // FS additions
                            if (pawn.gender == Gender.Male)
                            {
                                faceComp.BeardDef = this.NewBeard;
                            }

                            if (faceComp.HasSameBeardColor)
                            {
                                faceComp.BeardColor = PawnHairColors_PostFix.DarkerBeardColor(pawn.story.hairColor);
                            }

                            faceComp.BeardColor = this.NewBeardColour;

                            faceComp.EyeDef = this.NewEye;
                            faceComp.BrowDef = this.NewBrow;
                        }

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


        #region Drawer

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

                if (!faceComp.HasSameBeardColor)
                {
                    colourWrapper.Color = this.NewBeardColour;
                    Find.WindowStack.Add(
                        new Dialog_ColorPicker(
                            colourWrapper,
                            delegate { this.NewBeardColour = colourWrapper.Color; },
                            false,
                            true)
                        {
                            initialPosition = new Vector2(
                                    this.windowRect.xMax + _margin,
                                    this.windowRect.yMin),
                        });
                }
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

        private void DrawHairColorPickerCell(Color color, Rect rect, string colorName)
        {
            Widgets.DrawBoxSolid(rect, color);
            string text = colorName;
            Widgets.DrawHighlightIfMouseover(rect);
            if (color == this.NewHairColour)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
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
                        delegate { this.NewHairColour = colourWrapper.Color; },
                        false,
                        true)
                    {
                        initialPosition = new Vector2(this.windowRect.xMax + _margin, this.windowRect.yMin)
                    });
            }
        }

        private void DrawBeardColorPickerCell(Color color, Rect rect, string colorName)
        {
            Widgets.DrawBoxSolid(rect, color);
            string text = colorName;
            Widgets.DrawHighlightIfMouseover(rect);
            if (color == this.NewBeardColour)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
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
                        delegate { this.NewBeardColour = colourWrapper.Color; },
                        false,
                        true)
                    {
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
            GUI.DrawTexture(rect, this.RightEyeGraphic(eye).MatFront.mainTexture);
            GUI.DrawTexture(rect, this.LeftEyeGraphic(eye).MatFront.mainTexture);
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

                colourWrapper.Color = this.NewHairColour;

                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        colourWrapper,
                        delegate { this.NewHairColour = colourWrapper.Color; },
                        false,
                        true)
                    {
                        initialPosition = new Vector2(this.windowRect.xMax + _margin, this.windowRect.yMin)
                    });
            }
        }

        #endregion

        private void DrawUI(Rect parentRect)
        {
            GUI.BeginGroup(parentRect);
            string nameStringShort = pawn.NameStringShort;
            Vector2 vector = Text.CalcSize(nameStringShort);
            Rect pawnHeadRect = new Rect(0f, -10f, _previewSize, _previewSize);
            Rect eyeRect = new Rect(pawnHeadRect);
            eyeRect.width /= 2;
            eyeRect.height /= 2;
            eyeRect.x += pawnHeadRect.width / 4;
            eyeRect.y += pawnHeadRect.height / 4;

            if (faceComp.crownType == CrownType.Narrow)
            {
                float dist = eyeRect.width * 0.1f;
                eyeRect.width *= 0.8f;
                eyeRect.x += dist;
            }

            Rect pawnMouthRect = new Rect(eyeRect);
            pawnMouthRect.y += 30f;

            // pawnMouthRect.y += 10f;
            Rect pawnRect = new Rect(0f, 0f, _previewSize, _previewSize);
            Rect labelRect = new Rect(0f, pawnRect.yMax - vector.y-10f, vector.x, vector.y);
            Rect melaninRect = new Rect(2f, labelRect.yMax + _margin, _previewSize - 5f, 65f);
            Rect selectionRect = new Rect(0f, melaninRect.yMax + _margin, _previewSize, _previewSize);
            Rect listRect = new Rect(_previewSize + _margin, 0f, _listWidth, parentRect.height - _margin * 2);

            labelRect = labelRect.CenteredOnXIn(pawnRect);
            for (int i = 0; i < DisplayGraphics.Length; i++)
            {
                if (DisplayGraphics[i]?.graphic?.MatFront == null)
                {
                    continue;
                }
                {
                    // layer 1-5 = body
                    if (i <= (int)enums.GraphicSlotGroup.Shell)
                    {
                        {
                            DisplayGraphics[i].Draw(pawnRect);
                        }
                    }
                    else
                    {
                        switch (i)
                        {
                            case (int)enums.GraphicSlotGroup.Beard:
                                {
                                    if (pawn.gender == Gender.Male)
                                    {
                                        DisplayGraphics[i].Draw(pawnHeadRect);
                                    }

                                    break;
                                }

                            case (int)enums.GraphicSlotGroup.LeftEye:
                            case (int)enums.GraphicSlotGroup.RightEye:
                            case (int)enums.GraphicSlotGroup.Brows:
                                {
                                    DisplayGraphics[i].Draw(eyeRect);
                                }

                                break;
                            case (int)enums.GraphicSlotGroup.Mouth:
                                if (faceComp != null && faceComp.drawMouth)
                                {
                                    if (this.NewBeard.drawMouth || faceComp.HasNaturalMouth)
                                        DisplayGraphics[i].Draw(pawnMouthRect);
                                }

                                break;
                            default:
                                DisplayGraphics[i].Draw(pawnHeadRect);
                                break;
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
            // if (DisplayGraphics[6].graphic != null)
            // {
            // DisplayGraphics[6].Draw(pawnRect);
            // }
            // }
            // DisplayGraphics[6].Draw(apparelRect);
            GUI.DrawTexture(
                new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height),
                _nameBackground);
            Widgets.Label(labelRect, nameStringShort);
            float width = _previewSize;

            // float spacing = 10f;
            if (faceComp != null)
            {
                this.DrawHumanlikeColorSelector(melaninRect);
            }


            Widgets.DrawMenuSection(listRect);

            Rect set = new Rect(selectionRect)
                           {
                               height = 30f, width = selectionRect.width / 2 - 10f
                           };
            set.y += 10f;
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
            if (faceComp != null)
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

            if (faceComp != null)
                if (Widgets.ButtonText(set, "Eye"))
                {
                    this.Page = "eye";
                }

            set.x += set.width + 10f;

            if (faceComp != null)
                if (Widgets.ButtonText(set, "Brow"))
                {
                    this.Page = "brow";
                }

            set.y += 36f;
            set.x = selectionRect.x;
            set.width = selectionRect.width - 5f;
            if (faceComp != null)
            {
                Widgets.CheckboxLabeled(set, "Draw colonist mouth if suitable", ref faceComp.drawMouth);
                if (pawn.gender == Gender.Male)
                {
                    set.y += 24f;
                    Widgets.CheckboxLabeled(set, "Use same color for hair + beard", ref faceComp.HasSameBeardColor);
                }

                if (GUI.changed)
                {
                    if (faceComp.HasSameBeardColor) this.NewBeardColour = PawnHairColors_PostFix.DarkerBeardColor(this.NewHairColour);
                }
            }

            set.width = selectionRect.width / 2 - 10f;

            set.y += 36f;
            set.x = selectionRect.x;

            if (this.Page == "hair")
            {
                set.x = selectionRect.x;
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

            if (faceComp != null)
            {

                if (this.Page == "eye")
                {
                    set.x = selectionRect.x;
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

                if (this.Page == "eye")
                {
                    this.DrawEyePicker(listRect);
                }
            }

            if (this.Page == "hair")
            {
                set.y += 36f;
                set.width = selectionRect.width / 7.5f - 10f;
                set.x = selectionRect.x;

                this.DrawHairColorPickerCell(originalColour, set, "Original");
                set.x += set.width * 1.5f + 10f;

                int row = 0;
                int count = 0;

                foreach (Color color in PawnHairColors_PostFix.HairColors)
                {
                    this.DrawHairColorPickerCell(color, set, color.ToString());
                    set.x += set.width + 10f;

                    count++;
                    if (count >= 6 && row == 0)
                    {
                        set.y += 36f;
                        set.x = selectionRect.x;
                        set.width = selectionRect.width / 10f - 10f;
                        row++;
                        count = 0;
                    }
                }
            }

            if (faceComp != null && !faceComp.HasSameBeardColor)
            {
                if (this.Page == "beard")
                {
                    set.width = selectionRect.width / 7.5f - 10f;
                    set.x = selectionRect.x;

                    this.DrawBeardColorPickerCell(this.NewHairColour, set, "Hair color");
                    set.x += set.width * 1.5f + 10f;

                    int row = 0;
                    int count = 0;

                    foreach (Color color in PawnHairColors_PostFix.HairColors)
                    {
                        this.DrawBeardColorPickerCell(color, set, color.ToString());
                        set.x += set.width + 10f;

                        count++;
                        if (count >= 6 && row == 0)
                        {
                            set.y += 36f;
                            set.x = selectionRect.x;
                            set.width = selectionRect.width / 10f - 10f;
                            row++;
                            count = 0;
                        }
                    }
                }
            }

            GUI.EndGroup();
        }


        private enums.GraphicSlotGroup GetSlotForApparel(Thing apparel)
        {
            ApparelProperties apparel2 = apparel.def.apparel;
            ApparelLayer lastLayer = apparel2.LastLayer;
            enums.GraphicSlotGroup result;
            switch (lastLayer)
            {
                case ApparelLayer.OnSkin:
                    if (apparel2.bodyPartGroups.Count == 1
                        && apparel2.bodyPartGroups[0].Equals(BodyPartGroupDefOf.Legs))
                    {
                        result = enums.GraphicSlotGroup.OnSkinOnLegs;
                        return result;
                    }

                    result = enums.GraphicSlotGroup.OnSkin;
                    return result;
                case ApparelLayer.Middle:
                    result = enums.GraphicSlotGroup.Middle;
                    return result;
                case ApparelLayer.Shell:
                    result = enums.GraphicSlotGroup.Shell;
                    return result;
                case ApparelLayer.Overhead:
                    result = enums.GraphicSlotGroup.Overhead;
                    return result;
            }
            Log.Warning("Could not resolve 'LastLayer' " + lastLayer);
            result = enums.GraphicSlotGroup.Invalid;
            return result;
        }

        private static Vector2 SwatchSize = new Vector2(13, 14);
        private static Vector2 SwatchSpacing = new Vector2(20, 20);
        private static Color ColorSwatchBorder = new Color(0.77255f, 0.77255f, 0.77255f);
        private static Color ColorSwatchSelection = new Color(0.9098f, 0.9098f, 0.9098f);


        // Blatantly stolen from Prepare Carefully
        private void DrawHumanlikeColorSelector(Rect melaninRect)
        {
            int currentSwatchIndex = PawnSkinColors_FS.GetSkinDataIndexOfMelanin(this.NewMelanin);
            Color currentSwatchColor = PawnSkinColors_FS._SkinColors[currentSwatchIndex].color;

            Rect swatchRect = new Rect(melaninRect.x, melaninRect.y, SwatchSize.x, SwatchSize.y);

            // Draw the swatch selection boxes.
            int colorCount = PawnSkinColors_FS._SkinColors.Length;
            int clickedIndex = -1;
            for (int i = 0; i < colorCount; i++)
            {
                Color color = PawnSkinColors_FS._SkinColors[i].color;

                // If the swatch is selected, draw a heavier border around it.
                bool isThisSwatchSelected = i == currentSwatchIndex;
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
                    if (Widgets.ButtonInvisible(swatchRect))
                    {
                        clickedIndex = i;

                        // currentSwatchColor = color;
                    }
                }

                // Advance the swatch rect cursor position and wrap it if necessary.
                swatchRect.x += SwatchSpacing.x;
                if (swatchRect.x >= melaninRect.width - SwatchSize.x)
                {
                    swatchRect.y += SwatchSpacing.y;
                    swatchRect.x = melaninRect.x;
                }
            }

            // Draw the current color box.
            GUI.color = Color.white;
            Rect currentColorRect = new Rect(melaninRect.x, swatchRect.y + 4, 25, 25);
            if (swatchRect.x != melaninRect.x)
            {
                currentColorRect.y += SwatchSpacing.y;
            }

            GUI.color = ColorSwatchBorder;
            GUI.DrawTexture(currentColorRect, BaseContent.WhiteTex);
            GUI.color = PawnSkinColors.GetSkinColor(this.NewMelanin);
            GUI.DrawTexture(currentColorRect.ContractedBy(1), BaseContent.WhiteTex);
            GUI.color = Color.white;

            // Figure out the lerp value so that we can draw the slider.
            float minValue = 0.00f;
            float maxValue = 0.99f;
            float t = PawnSkinColors_FS.GetRelativeLerpValue(this.NewMelanin);
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
            float newValue = GUI.HorizontalSlider(new Rect(currentColorRect.x + 35, currentColorRect.y + 18, 136, 16), t, minValue, 1);
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

                float melaninLevel = PawnSkinColors_FS.GetValueFromRelativeLerp(currentSwatchIndex, newValue);
                this.NewMelanin = melaninLevel;
            }
        }


        private void SetGraphicSlot(
            enums.GraphicSlotGroup slotIndex,
            Thing newThing,
            Graphic newGraphic,
            Texture2D newIcon,
            Color newColor)
        {
            if (slotIndex < enums.GraphicSlotGroup.Head)
            {
                DisplayGraphics[(int)slotIndex] = new GraphicsDisp(newThing, newGraphic, newIcon, newColor, 0f);
            }

            // else if (slotIndex >= GraphicSlotGroup.Hair)
            // {
            // DisplayGraphics[(int)slotIndex] = new GraphicsDisp(newThing, newGraphic, newIcon, newColor, -40f);
            // }

            // if (slotIndex == GraphicSlotGroup.Head)
            else
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