namespace FacialStuff.FaceStyling_Bench
{
    using System.Collections.Generic;
    using System.Linq;

    using FaceStyling;

    using FacialStuff;
    using FacialStuff.ColorPicker;
    using FacialStuff.Defs;
    using FacialStuff.Detouring;
    using FacialStuff.Genetics;
    using FacialStuff.Graphics_FS;
    using FacialStuff.Utilities;

    using RimWorld;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public partial class Dialog_FaceStyling : Window
    {
        #region Fields

        private static readonly int _columns;
        private static readonly float _entrySize;
        private static readonly List<BeardDef> _fullBeardDefs;

        private static readonly float _iconSize;
        private static readonly float _listWidth;
        private static readonly List<BeardDef> _lowerBeardDefs;

        // private static Texture2D _icon;
        private static readonly float _margin;

        private static readonly List<MoustacheDef> _moustacheDefs;
        private static readonly Texture2D _nameBackground;

        private static readonly float _previewSize;

        private static readonly string _title;

        private static readonly float _titleHeight;

        private static List<BrowDef> _browDefs;

        private static List<EyeDef> _eyeDefs;

        private static List<HairDef> _hairDefs;

        private static BeardDef _newBeard;

        private static Color _newBeardColour;
        private static BrowDef _newBrow;
        private static EyeDef _newEye;
        private static HairDef _newHair;
        private static Color _newHairColour;
        private static MoustacheDef _newMoustache;
        private static Color ColorSwatchBorder = new Color(0.77255f, 0.77255f, 0.77255f);
        private static Color ColorSwatchSelection = new Color(0.9098f, 0.9098f, 0.9098f);
        private static ColorWrapper colourWrapper;

        private static GraphicsDisp[] DisplayGraphics;

        private static CompFace faceComp;

        private static BeardDef originalBeard;

        private static BrowDef originalBrow;
        private static Color originalColour;
        private static EyeDef originalEye;
        private static HairDef originalHair;
        private static MoustacheDef originalUpperBeard;
        private static Pawn pawn;

        private static Vector2 SwatchSize = new Vector2(13, 14);
        private static Vector2 SwatchSpacing = new Vector2(20, 20);
        private static List<string> vanillaHairTags = new List<string>
                                                          {
                                                              "Urban",
                                                              "Rural",
                                                              "Punk",
                                                              "Tribal"
                                                          };

        private readonly float originalMelanin;

        private float _newMelanin;

        private Vector2 _scrollPosition = Vector2.zero;

        private DialoguePage Page = DialoguePage.Hair;

        #endregion Fields

        #region Constructors

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

            _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairTags.SharesElementWith(vanillaHairTags));
            _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading;
            _fullBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType == BeardType.FullBeard).ToList();
            _lowerBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType != BeardType.FullBeard).ToList();
            _moustacheDefs = DefDatabase<MoustacheDef>.AllDefsListForReading;

            _browDefs = DefDatabase<BrowDef>.AllDefsListForReading;
            _fullBeardDefs.SortBy(i => i.LabelCap);
            _lowerBeardDefs.SortBy(i => i.LabelCap);
            _moustacheDefs.SortBy(i => i.LabelCap);
        }

        public Dialog_FaceStyling(Pawn p)
        {
            pawn = p;
            faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp != null)
            {
                if (faceComp.BeardDef == null)
                {
                    faceComp.BeardDef = BeardDefOf.Beard_Shaved;
                }

                originalColour = faceComp.HairColorOrg;
                _newBeardColour = faceComp.BeardColor;
                _newBeard = originalBeard = faceComp.BeardDef;
                _newMoustache = originalUpperBeard = faceComp.MoustacheDef;
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


            DisplayGraphics = new GraphicsDisp[(int)Enums.GraphicSlotGroup.NumberOfTypes];
            for (int i = 0; i < (int)Enums.GraphicSlotGroup.NumberOfTypes; i++)
            {
                DisplayGraphics[i] = new GraphicsDisp();
            }


            this.SetGraphicSlot(
                Enums.GraphicSlotGroup.Body,
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
                    Enums.GraphicSlotGroup.Head,
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
                    Enums.GraphicSlotGroup.Head,
                    pawn,
                   pawn.Drawer.renderer.graphics.headGraphic,
                    pawn.def.uiIcon,
                    pawn.story.SkinColor);
            }

            this.SetGraphicSlot(
                Enums.GraphicSlotGroup.Hair,
                pawn,
                pawn.Drawer.renderer.graphics.hairGraphic,
                pawn.def.uiIcon,
                pawn.story.hairColor);

            if (faceComp != null)
            {

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.RightEye,
                    pawn,
                    this.RightEyeGraphic(faceComp.EyeDef),
                    pawn.def.uiIcon,
                    Color.black);

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.LeftEye,
                    pawn,
                    this.LeftEyeGraphic(faceComp.EyeDef),
                    pawn.def.uiIcon,
                    Color.black);

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.Brows,
                    pawn,
                    this.BrowGraphic(faceComp.BrowDef),
                    pawn.def.uiIcon,
                    Color.black);

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.Mouth,
                    pawn,
                    faceComp.MouthGraphic,
                    pawn.def.uiIcon,
                   faceComp.HasNaturalMouth ? Color.black : Color.white);

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.Beard,
                    pawn,
                    this.BeardGraphic(faceComp.BeardDef),
                    pawn.def.uiIcon,
                    faceComp.BeardColor);
                if (faceComp.MoustacheDef != null)
                {

                    this.SetGraphicSlot(
                        Enums.GraphicSlotGroup.Moustache,
                        pawn,
                        this.MoustacheGraphic(faceComp.MoustacheDef),
                        pawn.def.uiIcon,
                        faceComp.BeardColor);
                }
            }

            foreach (Apparel current in pawn.apparel.WornApparel)
            {
                Enums.GraphicSlotGroup slotForApparel = this.GetSlotForApparel(current);
                if (slotForApparel != Enums.GraphicSlotGroup.Invalid)
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

        #endregion Constructors

        #region Properties

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(
                    _previewSize + _margin + _listWidth + 36f,
                    40f + _previewSize * 2f + _margin * 3f + 38f + 36f + 80f);
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

                if (value.beardType == BeardType.FullBeard)
                {
                    _newMoustache = MoustacheDefOf.Shaved;
                }

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.Beard,
                    pawn,
                    this.BeardGraphic(value),
                    pawn.def.uiIcon,
                    this.NewBeardColour);

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.Moustache,
                    pawn,
                    this.MoustacheGraphic(_newMoustache),
                    pawn.def.uiIcon,
                    this.NewBeardColour);
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
                DisplayGraphics[(int)Enums.GraphicSlotGroup.Beard].color = value;
                DisplayGraphics[(int)Enums.GraphicSlotGroup.Moustache].color = value;
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
                    Enums.GraphicSlotGroup.Brows,
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
                    Enums.GraphicSlotGroup.RightEye,
                    pawn,
                    this.RightEyeGraphic(value),
                    pawn.def.uiIcon,
                    Color.black);

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.LeftEye,
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
                    Enums.GraphicSlotGroup.Hair,
                    pawn,
                    this.HairGraphic(value),
                    pawn.def.uiIcon,
                    this.NewHairColour);
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
                DisplayGraphics[(int)Enums.GraphicSlotGroup.Hair].color = value;
                if (faceComp != null && faceComp.HasSameBeardColor)
                {
                    DisplayGraphics[(int)Enums.GraphicSlotGroup.Beard].color = FacialGraphics.DarkerBeardColor(value);
                    DisplayGraphics[(int)Enums.GraphicSlotGroup.Moustache].color = FacialGraphics.DarkerBeardColor(value);
                }
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
                    Enums.GraphicSlotGroup.Body,
                    pawn,
                    GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                        pawn.story.bodyType,
                        ShaderDatabase.CutoutSkin,
                        PawnSkinColors.GetSkinColor(this._newMelanin)),
                    pawn.def.uiIcon,
                    PawnSkinColors.GetSkinColor(this._newMelanin));

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.Head,
                    pawn,
                    GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(
                        pawn,
                        PawnSkinColors.GetSkinColor(this._newMelanin)),
                    pawn.def.uiIcon,
                    PawnSkinColors.GetSkinColor(this._newMelanin));
            }
        }

        public MoustacheDef NewMoustache
        {
            get
            {
                return _newMoustache;
            }

            set
            {
                _newMoustache = value;

                if (_newBeard.beardType == BeardType.FullBeard)
                {
                    _newBeard = PawnFaceChooser.RandomBeardDefFor(pawn, BeardType.LowerBeard);
                }

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.Moustache,
                    pawn,
                    this.MoustacheGraphic(value),
                    pawn.def.uiIcon,
                    this.NewBeardColour);

                this.SetGraphicSlot(
                    Enums.GraphicSlotGroup.Beard,
                    pawn,
                    this.BeardGraphic(_newBeard),
                    pawn.def.uiIcon,
                    this.NewBeardColour);
            }
        }

        #endregion Properties

        #region Methods

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
                                faceComp.MoustacheDef = this.NewMoustache;
                            }

                            if (faceComp.HasSameBeardColor)
                            {
                                faceComp.BeardColor = FacialGraphics.DarkerBeardColor(pawn.story.hairColor);
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
                        x => x.hairTags.SharesElementWith(vanillaHairTags)
                             && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    _browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    break;
                case Gender.Female:
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairTags.SharesElementWith(vanillaHairTags) &&
                         (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
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

        private Graphic_Multi_NaturalHeadParts BeardGraphic(BeardDef def)
        {
            Graphic_Multi_NaturalHeadParts result;
            if (def.texPath != null)
            {
                string path = def.texPath + "_" + faceComp.PawnCrownType + "_" + faceComp.PawnHeadType;
                if (def == BeardDefOf.Beard_Shaved)
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

        private void DrawBeardPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num7 = Mathf.CeilToInt(_fullBeardDefs.Count / (float)_columns);
            int num8 = Mathf.CeilToInt(_moustacheDefs.Count / (float)_columns);
            int num9 = Mathf.CeilToInt(_lowerBeardDefs.Count / (float)_columns);

            int nums = num7 + num8 + num9;
            rect3.height = nums * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / _columns;
                vector.y -= 16f / _columns;
                rect3.width -= 16f;
                rect3.height = vector.y * nums;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this._scrollPosition, rect3);
            GUI.BeginGroup(rect3);

            float curY = 0f;
            float thisY = 0f;
            for (int i = 0; i < _moustacheDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawMoustachePickerCell(_moustacheDefs[i], rect4);
                thisY = rect4.yMax;
            }

            curY = thisY;
            for (int i = 0; i < _lowerBeardDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y + curY, vector.x, vector.y);
                this.DrawBeardPickerCell(_lowerBeardDefs[i], rect4);
                thisY = rect4.yMax;
            }

            curY = thisY;

            for (int i = 0; i < _fullBeardDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y + curY, vector.x, vector.y);
                this.DrawBeardPickerCell(_fullBeardDefs[i], rect4);
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawBeardPickerCell(BeardDef beard, Rect rect)
        {

            if (_newMoustache != MoustacheDefOf.Shaved)
            {
                if (beard.beardType == BeardType.FullBeard)
                {
                    Widgets.DrawBoxSolid(rect.ContractedBy(3f), new Color(0.8f, 0f, 0f, 0.3f));
                }
                else
                {
                    if (this.NewBeard == BeardDefOf.Beard_Shaved || this.NewMoustache != MoustacheDefOf.Shaved)
                    {
                        Widgets.DrawBoxSolid(rect.ContractedBy(3f), new Color(0.29f, 0.7f, 0.8f, 0.3f));
                    }
                    else
                    {
                        Widgets.DrawBoxSolid(rect.ContractedBy(3f), new Color(0.8f, 0.8f, 0.8f, 0.3f));
                    }
                }
            }

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

        private void DrawMoustachePickerCell(MoustacheDef beard, Rect rect)
        {

            if (_newBeard.beardType == BeardType.FullBeard)
            {
                Widgets.DrawBoxSolid(rect.ContractedBy(3f), new Color(0.8f, 0f, 0f, 0.3f));
            }
            else
            {
                if (this.NewMoustache == MoustacheDefOf.Shaved)
                {
                    Widgets.DrawBoxSolid(rect.ContractedBy(3f), new Color(0.29f, 0.7f, 0.8f, 0.3f));
                }
                else
                {
                    Widgets.DrawBoxSolid(rect.ContractedBy(3f), new Color(0.8f, 0.8f, 0.8f, 0.3f));
                }
            }


            GUI.DrawTexture(rect, this.MoustacheGraphic(beard).MatFront.mainTexture);
            string text = beard.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (beard == this.NewMoustache)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (beard == originalUpperBeard)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewMoustache = beard;
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

            if (faceComp.PawnCrownType == CrownType.Narrow)
            {
                float dist = eyeRect.width * 0.1f;
                eyeRect.width *= 0.8f;
                eyeRect.x += dist;
            }

            Rect pawnMouthRect = new Rect(eyeRect);
            pawnMouthRect.y += 30f;

            // pawnMouthRect.y += 10f;
            Rect pawnRect = new Rect(0f, 0f, _previewSize, _previewSize);
            Rect labelRect = new Rect(0f, pawnRect.yMax - vector.y - 10f, vector.x, vector.y);
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
                // layer 1-5 = body
                if (i <= (int)Enums.GraphicSlotGroup.Shell)
                {
                    {
                        DisplayGraphics[i].Draw(pawnRect);
                    }
                }
                else
                {
                    switch (i)
                    {
                        case (int)Enums.GraphicSlotGroup.Beard:
                            {
                                if (pawn.gender == Gender.Male)
                                {
                                    DisplayGraphics[i].Draw(pawnHeadRect);
                                }

                                break;
                            }

                        case (int)Enums.GraphicSlotGroup.LeftEye:
                        case (int)Enums.GraphicSlotGroup.RightEye:
                        case (int)Enums.GraphicSlotGroup.Brows:
                            {
                                DisplayGraphics[i].Draw(eyeRect);
                            }

                            break;
                        case (int)Enums.GraphicSlotGroup.Mouth:
                            if (faceComp != null && faceComp.DrawMouth)
                            {
                                if (this.NewBeard.drawMouth && (this.NewMoustache == MoustacheDefOf.Shaved))
                                {
                                    DisplayGraphics[i].Draw(pawnMouthRect);
                                }
                            }

                            break;
                        default:
                            DisplayGraphics[i].Draw(pawnHeadRect);
                            break;
                    }
                }
            }

            GUI.DrawTexture(
                new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height),
                _nameBackground);
            Widgets.Label(labelRect, nameStringShort);


            // float spacing = 10f;
            if (faceComp != null)
            {
                this.DrawHumanlikeColorSelector(melaninRect);
            }

            Widgets.DrawMenuSection(listRect);

            Rect set = new Rect(selectionRect) { height = 30f, width = selectionRect.width / 2 - 10f };
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
                this.Page = DialoguePage.Hair;
            }

            set.x += set.width + 10f;
            if (faceComp != null)
                if (pawn.gender == Gender.Male)
                {
                    if (Widgets.ButtonText(set, "Beard"))
                    {
                        this.Page = DialoguePage.Beard;
                    }
                }

            set.y += 36f;
            set.width = selectionRect.width / 2 - 10f;
            set.x = selectionRect.x;

            if (faceComp != null)
                if (Widgets.ButtonText(set, "Eye"))
                {
                    this.Page = DialoguePage.Eye;
                }

            set.x += set.width + 10f;

            if (faceComp != null)
                if (Widgets.ButtonText(set, "Brow"))
                {
                    this.Page = DialoguePage.Brow;
                }

            set.y += 36f;
            set.x = selectionRect.x;
            set.width = selectionRect.width - 5f;
            if (faceComp != null)
            {
                bool faceCompDrawMouth = faceComp.DrawMouth;
                Widgets.CheckboxLabeled(set, "Draw colonist mouth if suitable", ref faceCompDrawMouth);
                faceComp.DrawMouth = faceCompDrawMouth;
                if (pawn.gender == Gender.Male)
                {
                    set.y += 24f;
                    bool faceCompHasSameBeardColor = faceComp.HasSameBeardColor;
                    Widgets.CheckboxLabeled(set, "Use same color for hair + beard", ref faceCompHasSameBeardColor);
                    faceComp.HasSameBeardColor = faceCompHasSameBeardColor;
                }

                if (GUI.changed)
                {
                    if (faceComp.HasSameBeardColor)
                        this.NewBeardColour = FacialGraphics.DarkerBeardColor(this.NewHairColour);
                }
            }

            set.width = selectionRect.width / 2 - 10f;

            set.y += 36f;
            set.x = selectionRect.x;

            if (this.Page == DialoguePage.Hair)
            {
                set.x = selectionRect.x;
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(selectionRect.x, set.y, selectionRect.width);
                GUI.color = Color.white;
                set.y += 12f;
                set.width = selectionRect.width / 3 - 10f;
                if (Widgets.ButtonText(set, "Female"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairTags.SharesElementWith(vanillaHairTags) &&
                         (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
                    _hairDefs.SortBy(i => i.LabelCap);
                }

                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Male"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairTags.SharesElementWith(vanillaHairTags) && (
                        x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                    _hairDefs.SortBy(i => i.LabelCap);
                }

                set.x += set.width + 10f;
                if (Widgets.ButtonText(set, "Any"))
                {
                    _hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(x => x.hairTags.SharesElementWith(vanillaHairTags) && x.hairGender == HairGender.Any);
                    _hairDefs.SortBy(i => i.LabelCap);
                }

                this.DrawHairPicker(listRect);
            }

            if (faceComp != null)
            {
                if (this.Page == DialoguePage.Eye)
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

                if (this.Page == DialoguePage.Brow)
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

                if (this.Page == DialoguePage.Beard)
                {
                    this.DrawBeardPicker(listRect);
                }

                if (this.Page == DialoguePage.Eye)
                {
                    this.DrawEyePicker(listRect);
                }
            }

            if (this.Page == DialoguePage.Hair)
            {
                set.y += 36f;
                set.width = selectionRect.width / 7.5f - 10f;
                set.x = selectionRect.x;

                this.DrawHairColorPickerCell(originalColour, set, "Original");
                set.x += set.width * 1.5f + 10f;

                foreach (Color color in HairMelanin.NaturalHairColors)
                {
                    this.DrawHairColorPickerCell(color, set, color.ToString());
                    set.x += set.width + 10f;
                }
                set.x = selectionRect.x;
                set.y += 36f;
                foreach (Color color in HairMelanin.ArtificialHairColors)
                {
                    this.DrawHairColorPickerCell(color, set, color.ToString());
                    set.x += set.width + 10f;
                }
            }

            if (faceComp != null && !faceComp.HasSameBeardColor)
            {
                if (this.Page == DialoguePage.Beard)
                {
                    set.width = selectionRect.width / 7.5f - 10f;
                    set.x = selectionRect.x;

                    this.DrawBeardColorPickerCell(this.NewHairColour, set, "Hair color");
                    set.x += set.width * 1.5f + 10f;

                    foreach (Color color in HairMelanin.NaturalHairColors)
                    {
                        this.DrawBeardColorPickerCell(color, set, color.ToString());
                        set.x += set.width + 10f;
                    }
                    set.x = selectionRect.x;
                    set.y += 36f;
                    foreach (Color color in HairMelanin.ArtificialHairColors)
                    {
                        this.DrawBeardColorPickerCell(color, set, color.ToString());
                        set.x += set.width + 10f;
                    }
                }
            }

            GUI.EndGroup();
        }

        private Enums.GraphicSlotGroup GetSlotForApparel(Thing apparel)
        {
            ApparelProperties apparel2 = apparel.def.apparel;
            ApparelLayer lastLayer = apparel2.LastLayer;
            Enums.GraphicSlotGroup result;
            switch (lastLayer)
            {
                case ApparelLayer.OnSkin:
                    if (apparel2.bodyPartGroups.Count == 1
                        && apparel2.bodyPartGroups[0].Equals(BodyPartGroupDefOf.Legs))
                    {
                        result = Enums.GraphicSlotGroup.OnSkinOnLegs;
                        return result;
                    }

                    result = Enums.GraphicSlotGroup.OnSkin;
                    return result;
                case ApparelLayer.Middle:
                    result = Enums.GraphicSlotGroup.Middle;
                    return result;
                case ApparelLayer.Shell:
                    result = Enums.GraphicSlotGroup.Shell;
                    return result;
                case ApparelLayer.Overhead:
                    result = Enums.GraphicSlotGroup.Overhead;
                    return result;
            }
            Log.Warning("Could not resolve 'LastLayer' " + lastLayer);
            result = Enums.GraphicSlotGroup.Invalid;
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

        private Graphic_Multi_NaturalEyes LeftEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes result;
            if (def.texPath != null)
            {
                string path = faceComp.EyeTexPath(def.texPath, Enums.Side.Left);


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

        private Graphic_Multi_NaturalHeadParts MoustacheGraphic(MoustacheDef def)
        {
            Graphic_Multi_NaturalHeadParts result;
            if (def.texPath != null)
            {
                string path = def.texPath;

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

        private Graphic_Multi_NaturalEyes RightEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes result;
            if (def.texPath != null)
            {
                string path = faceComp.EyeTexPath(def.texPath, Enums.Side.Right);

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
        private void SetGraphicSlot(
            Enums.GraphicSlotGroup slotIndex,
            Thing newThing,
            Graphic newGraphic,
            Texture2D newIcon,
            Color newColor)
        {
            if (slotIndex < Enums.GraphicSlotGroup.Head)
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

        #endregion Methods

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

        #region Classes

        // private void CheckSelectedFacePresetHasName()
        // {
        // if (SelectedFacePreset != null && SelectedFacePreset.label.NullOrEmpty())
        // {
        // SelectedFacePreset.label = "Unnamed";
        // }
        // }
        public class GraphicsDisp
        {
            #region Fields

            public Color color = Color.white;

            public Graphic graphic;

            public Texture2D icon;

            public Thing thing;

            private readonly float yOffset;

            #endregion Fields

            #region Constructors

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

            #endregion Constructors

            #region Properties

            public bool Valid
            {
                get
                {
                    return this.thing != null;
                }
            }

            #endregion Properties

            #region Methods

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

            #endregion Methods
        }

        #endregion Classes
    }
}