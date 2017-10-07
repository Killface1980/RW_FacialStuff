namespace FacialStuff.FaceStyling_Bench
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using FacialStuff.ColorPicker;
    using FacialStuff.Defs;
    using FacialStuff.Detouring;
    using FacialStuff.Enums;
    using FacialStuff.FaceStyling_Bench.UI.DTO;
    using FacialStuff.FaceStyling_Bench.UI.DTO.SelectionWidgetDTOs;
    using static FacialStuff.FaceStyling_Bench.UI.Util.WidgetUtil;
    using FacialStuff.Genetics;
    using FacialStuff.Graphics;
    using FacialStuff.Utilities;

    using global::FaceStyling;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public class DialogFaceStyling : Window
    {

        #region Public Fields

        public static Vector2 PortraitSize = new Vector2(203f, 203f);

        #endregion Public Fields

        // private void CheckSelectedFacePresetHasName()
        #region Private Fields

        private static readonly Color ColorSwatchBorder = new Color(0.77255f, 0.77255f, 0.77255f);

        private static readonly Color ColorSwatchSelection = new Color(0.9098f, 0.9098f, 0.9098f);

        private static int Columns;

        private static Color DarkBackground = new Color(0.12f, 0.12f, 0.12f);

        private static float EntrySize;

        private static List<BeardDef> FullBeardDefs;

        private static float ListWidth = 450f;

        private static List<BeardDef> LowerBeardDefs;

        // private static Texture2D _icon;
        private static float MarginFS = 14f;

        private static long MaxAge = 1000000000 * TicksPerYear;

        // private FacePreset SelectedFacePreset
        // {
        // get { return _selFacePresetInt; }
        // set
        // {
        // CheckSelectedFacePresetHasName();
        // _selFacePresetInt = value;
        // }
        // }
        private static List<MoustacheDef> MoustacheDefs;

        // private FacePreset _selFacePresetInt;
        private static Texture2D NameBackground;

        private static float PreviewSize = 220f;

        private static long TicksPerYear = 3600000L;

        private static string Title = "FacialStuffEditor.FaceStylerTitle".Translate();

        private static float TitleHeight = 30f;

        private static List<string> VanillaHairTags = new List<string> { "Urban", "Rural", "Punk", "Tribal" };

        private static List<BrowDef> browDefs;

        private static List<EyeDef> eyeDefs;

        private static List<HairDef> hairDefs;
        private static List<HairDef> filteredHairDefs;

        private static Pawn pawn;

        [NotNull]
        private CompFace faceComp;

        private bool hadSameBeardColor;
        private BeardTab beardTab;
        private ColorWrapper colourWrapper;
        private DresserDTO dresserDto;
        private GenderTab genderTab;
        private FilterTab filterTab;
        private SpecialTab specialTab;
        private bool hats;

        private bool initialized = false;

        private BeardDef newBeard;
        private Color newBeardColor;
        private BrowDef newBrow;
        private EyeDef newEye;
        private HairDef newHair;
        private Color newHairColor;
        private float newMelanin;
        private MoustacheDef newMoustache;
        private long originalAgeBio;

        private long originalAgeChrono;

        private BeardDef originalBeard;

        private Color originalBeardColor;

        private BodyType originalBodyType;

        private BrowDef originalBrow;

        private CrownType originalCrownType;

        private EyeDef originalEye;

        private Gender originalGender;

        private HairDef originalHair;

        private Color originalHairColor;

        private string originalHeadGraphicPath;

        private float originalMelanin;

        private MoustacheDef originalMoustache;
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private bool reInit;

        private bool rerenderPawn;

        private bool saveChangedOnExit;

        private List<Vector2> scrollPosition = new List<Vector2> { };

        private Vector2 scrollPositionBeard1 = Vector2.zero;

        private Vector2 scrollPositionBeard2 = Vector2.zero;

        private Vector2 scrollPositionBrow = Vector2.zero;

        private Vector2 scrollPositionEye = Vector2.zero;

        private Vector2 scrollPositionHairAny = Vector2.zero;

        private Vector2 scrollPositionHairAll = Vector2.zero;

        private Vector2 scrollPositionHairFemale = Vector2.zero;

        private Vector2 scrollPositionHairMale = Vector2.zero;

        private Vector2 swatchSize = new Vector2(14, 14);


        private FaceStyleTab tab;

        #endregion Private Fields

        #region Public Constructors

        static DialogFaceStyling()
        {
            // _previewSize = 100f;

            // _icon = ContentFinder<Texture2D>.Get("ClothIcon");

            // _listWidth = 200f;
            Columns = 12;
            EntrySize = ListWidth / Columns;
            NameBackground = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));
            currentFilter = new List<string> { "Urban", "Rural", "Punk", "Tribal" };
            HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                x => x.hairTags.SharesElementWith(VanillaHairTags));



            eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading;
            FullBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType == BeardType.FullBeard)
                .ToList();
            LowerBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType != BeardType.FullBeard)
                .ToList();
            MoustacheDefs = DefDatabase<MoustacheDef>.AllDefsListForReading;

            browDefs = DefDatabase<BrowDef>.AllDefsListForReading;
            FullBeardDefs.SortBy(i => i.LabelCap);
            LowerBeardDefs.SortBy(i => i.LabelCap);
            MoustacheDefs.SortBy(i => i.LabelCap);
            HairDefs.SortBy(i => i.LabelCap);
        }

        public DialogFaceStyling(Pawn p)
        {
            pawn = p;
            this.faceComp = pawn.TryGetComp<CompFace>();
            this.hats = Prefs.HatsOnlyOnMap;
            this.gear = Controller.settings.FilterHats;
            Prefs.HatsOnlyOnMap = true;
            Controller.settings.FilterHats = false;

            this.hadSameBeardColor = this.faceComp.PawnFace.HasSameBeardColor;

            if (pawn.gender == Gender.Female)
            {
                this.genderTab = GenderTab.Female;
            }
            else
            {
                this.genderTab = GenderTab.Male;
            }

            CurrentFilter = pawn.story.hairDef.hairTags;
            if (pawn.story.hairDef.hairTags.Contains("Urban"))
            {
                this.filterTab = FilterTab.Urban;
            }
            else if (pawn.story.hairDef.hairTags.Contains("Rural"))
            {
                this.filterTab = FilterTab.Rural;
            }
            else if (pawn.story.hairDef.hairTags.Contains("Punk"))
            {
                this.filterTab = FilterTab.Punk;
            }
            else
            {
                this.filterTab = FilterTab.Tribal;
            }

           // IIncidentTarget target = pawn.Map;
           // if (target != null)
           // {
           //     IncidentDef def = IncidentDef.Named("FacialStuffUpdateNote");
           //     StorytellerComp source = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_ThreatCycle || x is StorytellerComp_RandomMain);
           //     IncidentParms parms = source.GenerateParms(def.category, target);
           //     FiringIncident fi = new FiringIncident(def, source, parms);
           //     if (fi.def.Worker.CanFireNow(pawn.Map))
           //     {
           //         fi.def.Worker.TryExecute(parms);
           //         fi.parms.target.StoryState.Notify_IncidentFired(fi);
           //     }
           // }


            this.beardTab = this.faceComp.PawnFace.BeardDef.beardType == BeardType.FullBeard
                                ? BeardTab.FullBeards
                                : BeardTab.Combinable;

            this.newHairColor = this.originalHairColor = pawn.story.hairColor;
            this.newBeardColor = this.originalBeardColor = this.faceComp.PawnFace.BeardColor;
            this.newBeard = this.originalBeard = this.faceComp.PawnFace.BeardDef;
            this.newMoustache = this.originalMoustache = this.faceComp.PawnFace.MoustacheDef;
            this.newEye = this.originalEye = this.faceComp.PawnFace.EyeDef;
            this.newBrow = this.originalBrow = this.faceComp.PawnFace.BrowDef;


            this.colourWrapper = new ColorWrapper(Color.cyan);

            this.newMelanin = this.originalMelanin = pawn.story.melanin;

            this.newHair = this.originalHair = pawn.story.hairDef;

            this.originalBodyType = pawn.story.bodyType;
            this.originalGender = pawn.gender;
            this.originalHeadGraphicPath = pawn.story.HeadGraphicPath;
            this.originalCrownType = pawn.story.crownType;

            this.originalAgeBio = pawn.ageTracker.AgeBiologicalTicks;
            this.originalAgeChrono = pawn.ageTracker.AgeChronologicalTicks;

            this.wrinkles = this.faceComp.PawnFace.wrinkles;

            // this.absorbInputAroundWindow = false;
            this.closeOnClickedOutside = false;
            this.closeOnEscapeKey = true;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.rerenderPawn = true;
        }

        #endregion Public Constructors

        #region Private Enums

        private enum BeardTab : byte
        {
            Combinable,

            FullBeards
        }

        private enum FaceStyleTab : byte
        {
            Hair,

            Beard,

            Eye,

            Brow,

            TypeSelector,
        }

        private enum GenderTab : byte
        {
            Female,

            Male,

            Any,

            All
        }

        private enum FilterTab : byte
        {
            Urban,

            Rural,

            Punk,

            Tribal
        }

        private enum SpecialTab : byte
        {
            Head,

            Body,

            Any
        }

        #endregion Private Enums

        #region Public Properties

        public override Vector2 InitialSize => new Vector2(750f, 700f);

        public float NewMelanin
        {
            get => pawn.story.melanin;

            set
            {
                this.newMelanin = value;
                this.UpdatePawnColors(Color.green, value);
            }
        }

        public MoustacheDef NewMoustache
        {
            get => this.newMoustache;

            set
            {
                this.newMoustache = value;
                this.UpdatePawnDefs(value);

                if (this.newBeard.beardType == BeardType.FullBeard && !this.reInit)
                {
                    this.newBeard = PawnFaceMaker.RandomBeardDefFor(pawn, BeardType.LowerBeard);
                    this.UpdatePawnDefs(this.newBeard);
                }
            }
        }

        #endregion Public Properties

        #region Private Properties

        private BeardDef NewBeard
        {
            get => this.newBeard;

            set
            {
                this.newBeard = value;

                this.UpdatePawnDefs(value);
                if (value.beardType == BeardType.FullBeard && !this.reInit)
                {
                    this.newMoustache = MoustacheDefOf.Shaved;
                    this.UpdatePawnDefs(MoustacheDefOf.Shaved);
                }
            }
        }

        private Color NewBeardColor
        {
            get => this.newBeardColor;

            set
            {
                this.newBeardColor = value;
                this.UpdatePawnColors(this.NewBeard, value);
            }
        }

        private BrowDef NewBrow
        {
            get => this.newBrow;

            set
            {
                this.newBrow = value;
                this.UpdatePawnDefs(value);
            }
        }

        private EyeDef NewEye
        {
            get => this.newEye;

            set
            {
                this.newEye = value;

                this.UpdatePawnDefs(value);
            }
        }

        private HairDef NewHair
        {
            get => this.newHair;

            set
            {
                this.newHair = value;
                this.UpdatePawnDefs(value);
            }
        }

        private Color NewHairColor
        {
            get => this.newHairColor;

            set
            {
                this.newHairColor = value;
                this.UpdatePawnColors(this.NewHair, value);

                if (this.faceComp.PawnFace.HasSameBeardColor && !this.reInit)
                {
                    Color color = HairMelanin.DarkerBeardColor(value);
                    this.UpdatePawnColors(this.NewBeard, color);
                }
            }
        }

        public static List<HairDef> HairDefs
        {
            get
            {
                return hairDefs;
            }
            set
            {
                hairDefs = value;
                filteredHairDefs = hairDefs.FindAll(x => x.hairTags.SharesElementWith(CurrentFilter));
                filteredHairDefs.SortBy(i => i.LabelCap);
            }
        }

        public static List<string> CurrentFilter
        {
            get
            {
                return currentFilter;
            }
            set
            {
                currentFilter = value;
                filteredHairDefs = hairDefs.FindAll(x => x.hairTags.SharesElementWith(currentFilter));
                filteredHairDefs.SortBy(i => i.LabelCap);
            }
        }

        #endregion Private Properties

        #region Public Methods

        public static Rect AddPortraitWidget(float left, float top)
        {
            // Portrait
            Rect rect = new Rect(left, top, PortraitSize.x, PortraitSize.y);
            GUI.DrawTexture(rect, FaceTextures.gradient);

            // Draw the pawn's portrait
            GUI.BeginGroup(rect);
            Vector2 size = new Vector2(rect.height / 1.4f, rect.height); // 128x180
            Rect position = new Rect(
                rect.width * 0.5f - size.x * 0.5f,
                10f + rect.height * 0.5f - size.y * 0.5f,
                size.x,
                size.y);
            GUI.DrawTexture(position, PortraitsCache.Get(pawn, size, new Vector3(0f, 0f, 0.1f), 1.25f));

            // GUI.DrawTexture(position, PortraitsCache.Get(pawn, size, default(Vector3)));
            GUI.EndGroup();

            GUI.color = Color.white;
            Widgets.DrawBox(rect, 1);

            return rect;
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

            if (pawn.gender == Gender.Male)
            {
                TabRecord item2 = new TabRecord(
                    "FacialStuffEditor.Beard".Translate(),
                    this.SetTabFaceStyle(FaceStyleTab.Beard),
                    this.tab == FaceStyleTab.Beard);
                list.Add(item2);
            }

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

            Action nextAct = delegate
                {
                    this.saveChangedOnExit = true;

                    // SoundDef.Named("ShotShotgun").PlayOneShotOnCamera();
                    this.Close();
                };

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

            Action middleAct = delegate
                {
                    while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                    {
                    }

                    // HairDNA hair = HairMelanin.GenerateHairMelaninAndCuticula(pawn, Rand.Value > 0.5f);
                    this.reInit = true;
                    this.faceComp.PawnFace.HasSameBeardColor = Rand.Value > 0.3f;
                    this.NewHair = PawnHairChooser.RandomHairDefFor(pawn, Faction.OfPlayer.def);
                    this.faceComp.PawnFace.GenerateHairDNA(pawn, true);
                    this.NewHairColor = this.faceComp.PawnFace.HairColor;
                    this.NewBeardColor = this.faceComp.PawnFace.BeardColor;
                    PawnFaceMaker.RandomBeardDefFor(
                        pawn,
                        Faction.OfPlayer.def,
                        out BeardDef beard,
                        out MoustacheDef tache);
                    this.NewBeard = beard;
                    this.NewMoustache = tache;

                    this.reInit = false;

                    // SoundDef.Named("ShotPistol").PlayOneShot();
                };

            DialogUtility.DoNextBackButtons(
                inRect,
                "Randomize".Translate(),
                "FacialStuffEditor.Accept".Translate(),
                backAct,
                middleAct,
                nextAct);
        }

        private void RemoveColorPicker()
        {
            while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker), false))
            {
            }
        }

        public override void PostOpen()
        {
            FillDefs();
            hairDefs.SortBy(i => i.LabelCap);
            eyeDefs.SortBy(i => i.LabelCap);
            browDefs.SortBy(i => i.LabelCap);
        }

        public override void PreClose()
        {
            Prefs.HatsOnlyOnMap = this.hats;
            Controller.settings.FilterHats = this.gear;
            if (!this.saveChangedOnExit)
            {
                this.ResetPawnFace();
            }

            this.RemoveColorPicker();

        }

        #endregion Public Methods

        #region Private Methods

        // ReSharper disable once MethodTooLong
        private static void FillDefs()
        {
            switch (pawn.gender)
            {
                case Gender.Male:
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairTags.SharesElementWith(VanillaHairTags)
                             && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                    eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    break;

                case Gender.Female:
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairTags.SharesElementWith(VanillaHairTags)
                             && (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
                    eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    break;
            }
        }

        [NotNull]
        private Graphic_Multi_NaturalHeadParts BeardGraphic([NotNull] BeardDef def)
        {
            string path = this.faceComp.GetBeardPath(def);

            Graphic_Multi_NaturalHeadParts result = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                        path,
                                                        ShaderDatabase.Cutout,
                                                        new Vector2(38f, 38f),
                                                        Color.white,
                                                        Color.white) as Graphic_Multi_NaturalHeadParts;

            return result;
        }

        private Graphic_Multi_NaturalHeadParts BrowGraphic(BrowDef def)
        {
            Graphic_Multi_NaturalHeadParts result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                             this.faceComp.BrowTexPath(def),
                             ShaderDatabase.CutoutSkin,
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

        private void DoColorWindowBeard()
        {
            this.RemoveColorPicker();

            this.colourWrapper.Color = this.faceComp.PawnFace.HasSameBeardColor
                                           ? this.NewHairColor
                                           : this.NewBeardColor;
            Find.WindowStack.Add(
                new Dialog_ColorPicker(
                    this.colourWrapper,
                    delegate
                        {
                            if (this.faceComp.PawnFace.HasSameBeardColor)
                            {
                                this.NewHairColor = this.colourWrapper.Color;
                            }

                            this.NewBeardColor = this.colourWrapper.Color;
                        },
                    false,
                    true)
                { initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin), });
        }

        private void DrawBeardColorPickerCell(Color color, Rect rect, string colorName)
        {
            Widgets.DrawBoxSolid(rect, color);
            string text = colorName;
            Widgets.DrawHighlightIfMouseover(rect);
            if (color == this.NewBeardColor)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                this.RemoveColorPicker();


                this.colourWrapper.Color = color;
                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        this.colourWrapper,
                        delegate
                            {
                                if (this.faceComp.PawnFace.HasSameBeardColor)
                                {
                                    this.NewHairColor = this.colourWrapper.Color;
                                }

                                this.NewBeardColor = this.colourWrapper.Color;
                            },
                        false,
                        true)
                    {
                        initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                    });
            }
        }

        private void DrawBeardPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                "FacialStuffEditor.FullBeards".Translate(),
                delegate
                    {
                        HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
                                 && (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
                        HairDefs.SortBy(i => i.LabelCap);
                        this.beardTab = BeardTab.FullBeards;
                    },
                this.beardTab == BeardTab.FullBeards);
            list.Add(item);

            TabRecord item2 = new TabRecord(
                "FacialStuffEditor.Combinable".Translate(),
                delegate
                    {
                        HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
                                 && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                        HairDefs.SortBy(i => i.LabelCap);
                        this.beardTab = BeardTab.Combinable;
                    },
                this.beardTab == BeardTab.Combinable);
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

            if (this.beardTab == BeardTab.Combinable)
            {
                allRows = rowsTache + rowsLowerBeards;
            }
            else
            {
                allRows = rowsBeard;
            }

            rect3.height = allRows * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * 2, thisEntrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * allRows;
            }

            switch (this.beardTab)
            {
                case BeardTab.Combinable:
                    Widgets.BeginScrollView(rect2, ref this.scrollPositionBeard1, rect3);
                    break;
                case BeardTab.FullBeards:
                    Widgets.BeginScrollView(rect2, ref this.scrollPositionBeard2, rect3);
                    break;
            }
            GUI.BeginGroup(rect3);

            float curY = 0f;
            float thisY = 0f;
            if (this.beardTab == BeardTab.Combinable)
            {
                for (int i = 0; i < MoustacheDefs.Count; i++)
                {
                    int yPos = i / thisColumns;
                    int xPos = i % thisColumns;
                    Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                    this.DrawMoustachePickerCell(MoustacheDefs[i], rect4.ContractedBy(3f));
                    thisY = rect4.yMax;
                }

                curY = thisY;
                for (int i = 0; i < LowerBeardDefs.Count; i++)
                {
                    int num2 = i / thisColumns;
                    int num3 = i % thisColumns;
                    Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y + curY, vector.x, vector.y);
                    this.DrawBeardPickerCell(LowerBeardDefs[i], rect4.ContractedBy(3f));
                }
            }

            if (this.beardTab == BeardTab.FullBeards)
            {
                for (int i = 0; i < FullBeardDefs.Count; i++)
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

        private void DrawBeardPickerCell([NotNull] BeardDef beard, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = beard.LabelCap;
            float offset = (rect.width / 2 - rect.height) / 3;
            {
                // Highlight box
                Widgets.DrawHighlightIfMouseover(rect);
                if (beard == this.NewBeard)
                {
                    Widgets.DrawHighlightSelected(rect);
                    text += "\n(selected)";
                }
                else
                {
                    if (beard == this.originalBeard)
                    {
                        Widgets.DrawAltRect(rect);
                        text += "\n(original)";
                    }
                }
            }

            if (this.newMoustache != MoustacheDefOf.Shaved)
            {
                {
                    // if (beard.beardType == BeardType.FullBeard)
                    // {
                    // Widgets.DrawBoxSolid(rect, new Color(0.8f, 0f, 0f, 0.3f));
                    // }
                    // else
                    if (this.NewBeard == BeardDefOf.Beard_Shaved || this.NewMoustache != MoustacheDefOf.Shaved)
                    {
                        Widgets.DrawBoxSolid(rect, new Color(0.29f, 0.7f, 0.8f, 0.3f));
                    }
                    else
                    {
                        Widgets.DrawBoxSolid(rect, new Color(0.8f, 0.8f, 0.8f, 0.3f));
                    }
                }
            }

            // Get the offset, cause width != 2 * height
            Rect rect1 = new Rect(rect.x + offset, rect.y, rect.height, rect.height);
            Rect rect2 = new Rect(rect1.xMax + offset, rect.y, rect.height, rect.height);

            GUI.color = pawn.story.SkinColor;
            GUI.DrawTexture(rect1, pawn.Drawer.renderer.graphics.headGraphic.MatFront.mainTexture);
            GUI.DrawTexture(rect2, pawn.Drawer.renderer.graphics.headGraphic.MatSide.mainTexture);
            GUI.color = this.faceComp.PawnFace.HasSameBeardColor ? pawn.story.hairColor : this.NewBeardColor;
            GUI.DrawTexture(rect1, this.BeardGraphic(beard).MatFront.mainTexture);
            GUI.DrawTexture(rect2, this.BeardGraphic(beard).MatSide.mainTexture);
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewBeard = beard;
                this.DoColorWindowBeard();
            }
        }

        private void DrawBrowPicker(Rect rect)
        {
            // 12 columns as base
            int divider = 3;
            int iconSides = 1;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(browDefs.Count / (float)thisColumns);

            rect3.height = num * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / Columns;
                vector.y -= 16f / Columns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this.scrollPositionBrow, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < browDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                this.DrawBrowPickerCell(browDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawBrowPickerCell(BrowDef brow, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = brow.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (brow == this.NewBrow)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (brow == this.originalBrow)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.color = Color.black;
            GUI.DrawTexture(rect, this.BrowGraphic(brow).MatFront.mainTexture);
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewBrow = brow;
                this.RemoveColorPicker();
            }
        }

        private void DrawEyePicker(Rect rect)
        {
            // 12 columns as base
            int divider = 3;
            int iconSides = 1;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(eyeDefs.Count / (float)thisColumns);

            rect3.height = num * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this.scrollPositionEye, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < eyeDefs.Count; i++)
            {
                int num2 = i / thisColumns;
                int num3 = i % thisColumns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawEyePickerCell(eyeDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawEyePickerCell(EyeDef eye, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = eye.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (eye == this.NewEye)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (eye == this.originalEye)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.DrawTexture(rect, this.RightEyeGraphic(eye).MatFront.mainTexture);
            GUI.DrawTexture(rect, this.LeftEyeGraphic(eye).MatFront.mainTexture);

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewEye = eye;
                this.RemoveColorPicker();
            }
        }

        private void DrawHairColorPickerCell(Color color, Rect rect, string colorName, [CanBeNull] HairColorRequest request = null)
        {
            string text = colorName;
            Widgets.DrawHighlightIfMouseover(rect);
            if (this.NewHairColor.IndistinguishableFrom(color))
            {
                this.DrawColorSelected(rect.ContractedBy(-2f));
                text += "\n(selected)";
            }

            Widgets.DrawBoxSolid(rect, color);

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                this.RemoveColorPicker();


                if (request != null)
                {
                    this.faceComp.PawnFace.PheoMelanin = request.PheoMelanin;
                    this.faceComp.PawnFace.EuMelanin = request.EuMelanin;
                }

                this.colourWrapper.Color = color;
                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        this.colourWrapper,
                        delegate { this.NewHairColor = this.colourWrapper.Color; },
                        false,
                        true)
                    {
                        initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                    });
            }
        }

        private void DrawSpecialPicker(Rect rect)
        {
            foreach (GraphicDatabaseHeadRecordsModded.HeadGraphicRecordVanillaCustom source in
                GraphicDatabaseHeadRecordsModded.HeadsVanillaCustom)
            {
                source.GetGraphic(pawn.story.SkinColor);
            }

            // string str = "Naked_" + bodyType.ToString();
            // string path = "Things/Pawn/Humanlike/Bodies/" + str;
            // return GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor);
            List<TabRecord> list = new List<TabRecord>();
            TabRecord item = new TabRecord(
                "HeadType".Translate(),
                delegate
                    {
                        HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
                                 && (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
                        HairDefs.SortBy(i => i.LabelCap);
                        this.specialTab = SpecialTab.Head;
                    },
                this.specialTab == SpecialTab.Head);
            list.Add(item);

            TabRecord item2 = new TabRecord(
                "BodyType".Translate(),
                delegate
                    {
                        HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
                                 && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                        HairDefs.SortBy(i => i.LabelCap);
                        this.specialTab = SpecialTab.Body;
                    },
                this.specialTab == SpecialTab.Body);
            list.Add(item2);

            TabRecord item3 = new TabRecord(
                "FacialStuffEditor.Any".Translate(),
                delegate
                    {
                        HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags) && x.hairGender == HairGender.Any);
                        HairDefs.SortBy(i => i.LabelCap);
                        this.genderTab = GenderTab.Any;
                    },
                this.specialTab == SpecialTab.Head);

            list.Add(item3);

            TabDrawer.DrawTabs(rect, list);

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;

            // 12 columns as base
            int divider = 3;
            int iconSides = 2;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            int rowsCount = Mathf.CeilToInt(HairDefs.Count / (float)thisColumns);

            rect3.height = rowsCount * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if (rect3.height > rect.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * rowsCount;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            switch (this.genderTab)
            {
                case GenderTab.Male:
                    Widgets.BeginScrollView(rect, ref this.scrollPositionHairMale, rect3);
                    break;
                case GenderTab.Female:
                    Widgets.BeginScrollView(rect, ref this.scrollPositionHairFemale, rect3);
                    break;
                case GenderTab.Any:
                    Widgets.BeginScrollView(rect, ref this.scrollPositionHairAny, rect3);
                    break;
            }
            GUI.BeginGroup(rect3);

            for (int i = 0; i < HairDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                this.DrawHairPickerCell(HairDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawHairPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                "Female".Translate(),
                delegate
                    {
                        HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
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
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
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
                            x => x.hairTags.SharesElementWith(VanillaHairTags) && x.hairGender == HairGender.Any);
                        HairDefs.SortBy(i => i.LabelCap);
                        this.genderTab = GenderTab.Any;
                    },
                this.genderTab == GenderTab.Any);
            list.Add(item3);

            TabRecord item4 = new TabRecord(
                "FacialStuffEditor.All".Translate(),
                delegate
                    {
                        HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags));
                        HairDefs.SortBy(i => i.LabelCap);
                        this.genderTab = GenderTab.All;
                    },
                this.genderTab == GenderTab.All);

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

            Rect rect2a = new Rect(rect);

            rect2a.yMin += 32f;

            TabDrawer.DrawTabs(rect2a, list2);


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

        private void DrawHairPickerCell(HairDef hair, Rect rect)
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
                if (hair == this.NewHair)
                {
                    Widgets.DrawHighlightSelected(rect);
                    label += "\n(selected)";
                }
                else
                {
                    if (hair == this.originalHair)
                    {
                        Widgets.DrawAltRect(rect);
                        label += "\n(original)";
                    }
                }
            }

            string highlightText = "";
            foreach (string hairTag in hair.hairTags)
            {
                if (!highlightText.NullOrEmpty())
                {
                    highlightText += "\n";
                }
                highlightText += hairTag;
            }

            // Rect rect3 = new Rect(rect2.xMax, rect.y, rect.height, rect.height);
            GUI.color = pawn.story.SkinColor;
            GUI.DrawTexture(rect1, pawn.Drawer.renderer.graphics.headGraphic.MatFront.mainTexture);
            GUI.DrawTexture(rect2, pawn.Drawer.renderer.graphics.headGraphic.MatSide.mainTexture);

            GUI.color = pawn.story.hairColor;
            GUI.DrawTexture(rect1, this.HairGraphic(hair).MatFront.mainTexture);
            GUI.DrawTexture(rect2, this.HairGraphic(hair).MatSide.mainTexture);

            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, highlightText);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewHair = hair;

                this.RemoveColorPicker();
                {
                    this.colourWrapper.Color = this.NewHairColor;

                    Find.WindowStack.Add(
                        new Dialog_ColorPicker(
                            this.colourWrapper,
                            delegate { this.NewHairColor = this.colourWrapper.Color; },
                            false,
                            true)
                        {
                            initialPosition = new Vector2(
                                    this.windowRect.xMax + MarginFS,
                                    this.windowRect.yMin)
                        });
                }
            }
        }

        // Blatantly stolen from Prepare Carefully
        private void DrawHumanlikeColorSelector(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.3f, 0.3f, 0.3f));
            Rect contractedBy = rect.ContractedBy(MarginFS / 2);

            GUI.BeginGroup(contractedBy);

            int currentSwatchIndex = PawnSkinColors_FS.GetSkinDataIndexOfMelanin(this.NewMelanin);

            int colorCount = PawnSkinColors_FS.SkinColors.Length;
            float size = contractedBy.width / (colorCount - 1);

            Rect swatchRect = new Rect(0, 0, size, size);

            // Draw the swatch selection boxes.
            int clickedIndex = -1;
            for (int i = 0; i < colorCount - 1; i++)
            {
                Color color = PawnSkinColors_FS.SkinColors[i].color;

                // If the swatch is selected, draw a heavier border around it.
                // bool isThisSwatchSelected = i == currentSwatchIndex;
                bool isThisSwatchSelected = this.NewMelanin >= PawnSkinColors_FS.SkinColors[i].melanin
                                            && this.NewMelanin
                                            < PawnSkinColors_FS.SkinColors[Mathf.Min(colorCount - 1, i + 1)].melanin;
                if (isThisSwatchSelected)
                {
                    this.DrawColorSelected(swatchRect);
                }

                // Draw the border around the swatch.
                // Rect borderRect = new Rect(
                // swatchRect.x - 1,
                // swatchRect.y - 1,
                // this.swatchSize.x + 2,
                // this.swatchSize.y + 2);
                // GUI.color = ColorSwatchBorder;
                // GUI.DrawTexture(borderRect, BaseContent.WhiteTex);

                // Draw the swatch itself.
                Widgets.DrawBoxSolid(swatchRect.ContractedBy(2f), color);

                if (!isThisSwatchSelected)
                {
                    if (Widgets.ButtonInvisible(swatchRect))
                    {
                        clickedIndex = i;
                    }
                }

                // Advance the swatch rect cursor position and wrap it if necessary.
                swatchRect.x += swatchRect.width;

                // no wrapping, only one row
                // if (swatchRect.x >= contractedBy.width - this.swatchSize.x)
                // {
                // swatchRect.y += size + MarginFS / 2;
                // swatchRect.x = 0;
                // }
            }

            Rect melaninSlider = new Rect(0, swatchRect.yMax + MarginFS / 2, contractedBy.width, SelectionRowHeight);
            float melly = this.NewMelanin;
            melly = Widgets.HorizontalSlider(
                melaninSlider,
                melly,
                0f,
                0.999f,
                false,
                "FacialStuffEditor.MelaninLevel".Translate(),
                "0",
                "1");



            // Draw the current color box.
            Rect currentColorRect = new Rect(0, melaninSlider.yMax + MarginFS, 20, 20);

            Widgets.DrawBoxSolid(currentColorRect, ColorSwatchBorder);
            Widgets.DrawBoxSolid(currentColorRect.ContractedBy(1), PawnSkinColors_FS.GetSkinColor(melly));

            // Figure out the lerp value so that we can draw the slider.
            float minValue = 0.00f;
            float maxValue = 0.999f;
            float t = PawnSkinColors_FS.GetRelativeLerpValue(melly);
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
            Rect detailRect = new Rect(
                currentColorRect.x + 35f,
                currentColorRect.y + MarginFS / 2,
                contractedBy.width - 35f,
                SelectionRowHeight);

            float newValue = Widgets.HorizontalSlider(detailRect, t, minValue, 1);
            if (newValue < minValue)
            {
                newValue = minValue;
            }
            else if (newValue > maxValue)
            {
                newValue = maxValue;
            }

            // If the user selected a new swatch or changed the lerp value, set a new color value.
            // ReSharper disable once InvertIf
            if (t != newValue || clickedIndex != -1)
            {
                if (clickedIndex != -1)
                {
                    currentSwatchIndex = clickedIndex;
                }

                float melaninLevel = PawnSkinColors_FS.GetValueFromRelativeLerp(currentSwatchIndex, newValue);
                melly = melaninLevel;
            }

            if (GUI.changed)
            {
                if (Math.Abs(melly - this.NewMelanin) > 0.001f)
                {
                    this.NewMelanin = melly;
                }
            }

            if (Controller.settings.UseWrinkles)
            {
                Rect wrinkleRect = new Rect(contractedBy.x, detailRect.yMax, contractedBy.width, SelectionRowHeight);

                float wrinkle = this.faceComp.PawnFace.wrinkles;

                wrinkle = Widgets.HorizontalSlider(
                    wrinkleRect,
                    wrinkle,
                    0f,
                    1f,
                    false,
                    "FacialStuffEditor.Wrinkles".Translate(),
                    "0",
                    "1");

                if (GUI.changed)
                {
                    if (Math.Abs(wrinkle - this.faceComp.PawnFace.wrinkles) > 0.001f)
                    {
                        this.faceComp.PawnFace.wrinkles = wrinkle;
                        this.rerenderPawn = true;
                    }
                }
            }

            GUI.EndGroup();

        }

        private void DrawColorSelected(Rect swatchRect)
        {
            Widgets.DrawBoxSolid(swatchRect, Color.white);
        }

        private void DrawMoustachePickerCell(MoustacheDef moustache, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);
            string text = moustache.LabelCap;
            float offset = (rect.width / 2 - rect.height) / 3;
            {
                // Highlight box
                Widgets.DrawHighlightIfMouseover(rect);
                if (moustache == this.NewMoustache)
                {
                    Widgets.DrawHighlightSelected(rect);
                    text += "\n(selected)";
                }
                else
                {
                    if (moustache == this.originalMoustache)
                    {
                        Widgets.DrawAltRect(rect);
                        text += "\n(original)";
                    }
                }
            }
            {
                // if (newBeard.beardType == BeardType.FullBeard)
                // {
                // Widgets.DrawBoxSolid(rect, new Color(0.8f, 0f, 0f, 0.3f));
                // }
                // else
                if (this.NewMoustache == MoustacheDefOf.Shaved)
                {
                    Widgets.DrawBoxSolid(rect, new Color(0.29f, 0.7f, 0.8f, 0.3f));
                }
                else
                {
                    Widgets.DrawBoxSolid(rect, new Color(0.8f, 0.8f, 0.8f, 0.3f));
                }
            }

            Rect rect1 = new Rect(rect.x + offset, rect.y, rect.height, rect.height);
            Rect rect2 = new Rect(rect1.xMax + offset, rect.y, rect.height, rect.height);

            GUI.color = pawn.story.SkinColor;
            GUI.DrawTexture(rect1, pawn.Drawer.renderer.graphics.headGraphic.MatFront.mainTexture);
            GUI.DrawTexture(rect2, pawn.Drawer.renderer.graphics.headGraphic.MatSide.mainTexture);
            GUI.color = this.faceComp.PawnFace.HasSameBeardColor ? pawn.story.hairColor : this.NewBeardColor;
            GUI.DrawTexture(rect1, this.MoustacheGraphic(moustache).MatFront.mainTexture);
            GUI.DrawTexture(rect2, this.MoustacheGraphic(moustache).MatSide.mainTexture);
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewMoustache = moustache;
                this.DoColorWindowBeard();
            }
        }

        private void DrawTypeSelector(Rect rect)
        {
            float editorLeft = rect.x;
            float editorTop = 30f + SelectionRowHeight;
            float editorWidth = 325f;

            float top = editorTop + 64f;

            AddSelectorWidget(
                editorLeft,
                top,
                editorWidth,
                "FacialStuffEditor.BodyType".Translate() + ":",
                this.dresserDto.BodyTypeSelectionDto);

            top += SelectionRowHeight + 20f;
            AddSelectorWidget(
                editorLeft,
                top,
                editorWidth,
                "FacialStuffEditor.HeadType".Translate() + ":",
                this.dresserDto.HeadTypeSelectionDto);

            top += SelectionRowHeight + 20f;

            if (Controller.settings.ShowGenderAgeChange)
            {
                GUI.Label(
                    new Rect(editorLeft, top, editorWidth, 64f),
                    "FacialStuffEditor.GenderChangeWarning".Translate());

                top += 64f + 20f;

                AddSelectorWidget(
                    editorLeft,
                    top,
                    editorWidth,
                    "FacialStuffEditor.Gender".Translate() + ":",
                    this.dresserDto.GenderSelectionDto);

                // top += WidgetUtil.SelectionRowHeight + 5;
                // long ageBio = pawn.ageTracker.AgeBiologicalTicks;
                // if (this.AddLongInput(
                // editorLeft,
                // top,
                // 120,
                // 80,
                // "FacialStuffEditor.AgeBiological".Translate() + ":",
                // ref ageBio,
                // MaxAge,
                // TicksPerYear))
                // {
                // pawn.ageTracker.AgeBiologicalTicks = ageBio;
                // this.rerenderPawn = true;
                // if (ageBio > pawn.ageTracker.AgeChronologicalTicks)
                // {
                // pawn.ageTracker.AgeChronologicalTicks = ageBio;
                // }
                // }
                // top += WidgetUtil.SelectionRowHeight + 5;
                // long ageChron = pawn.ageTracker.AgeChronologicalTicks;
                // if (this.AddLongInput(
                // editorLeft,
                // top,
                // 120,
                // 80,
                // "FacialStuffEditor.AgeChronological".Translate() + ":",
                // ref ageChron,
                // MaxAge,
                // TicksPerYear))
                // {
                // pawn.ageTracker.AgeChronologicalTicks = ageChron;
                // }
            }

            GUI.color = Color.white;
        }

        private bool skinPage = true;

        private float wrinkles;

        private Vector2 pickerPosition = Vector2.zero;

        private Vector2 pickerSize = new Vector2(200, 200);

        private bool gear;

        private static List<string> currentFilter;

        private void DrawUI(Rect rect)
        {
            GUI.BeginGroup(rect);
            string pawnName = pawn.NameStringShort;
            Vector2 vector = Text.CalcSize(pawnName);

            Rect pawnRect = AddPortraitWidget(0f, TitleHeight);
            Rect labelRect = new Rect(0f, pawnRect.yMax, vector.x, vector.y);
            labelRect = labelRect.CenteredOnXIn(pawnRect);

            float width = rect.width - ListWidth - MarginFS;

            Rect button = new Rect(
                0f,
                labelRect.yMax + MarginFS / 2,
                (width - MarginFS) / 2,
                SelectionRowHeight);
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

            Rect listRect = new Rect(0f, TitleHeight, ListWidth, height);
            listRect.x = mainRect.xMax + MarginFS;

            mainRect.yMax = listRect.yMax;

            this.pickerPosition = new Vector2(mainRect.position.x, mainRect.position.y);
            this.pickerSize = new Vector2(mainRect.width, mainRect.height);

            GUI.DrawTexture(
                new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height),
                NameBackground);
            Widgets.Label(labelRect, pawnName);

            Rect set = new Rect(mainRect) { height = SelectionRowHeight, width = mainRect.width / 2 - 10f };
            set.y = listRect.yMax - SelectionRowHeight;
            set.width = mainRect.width - MarginFS / 3;

            bool faceCompDrawMouth = this.faceComp.PawnFace.DrawMouth;
            bool faceCompHasSameBeardColor = this.faceComp.PawnFace.HasSameBeardColor;

            mainRect.yMax -= SelectionRowHeight + MarginFS;
            if (this.skinPage)
            {
                this.DrawHumanlikeColorSelector(mainRect);
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
                    this.DrawHairColorSelector(mainRect);
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
                if (this.faceComp.PawnFace.HasSameBeardColor != faceCompHasSameBeardColor)
                {
                    this.RemoveColorPicker();
                    this.faceComp.PawnFace.HasSameBeardColor = faceCompHasSameBeardColor;
                    this.NewBeardColor = HairMelanin.DarkerBeardColor(this.NewHairColor);
                }

                if (this.faceComp.PawnFace.DrawMouth != faceCompDrawMouth)
                {
                    this.faceComp.PawnFace.DrawMouth = faceCompDrawMouth;
                    this.rerenderPawn = true;
                }
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

            GUI.EndGroup();
        }

        public override void PostClose()
        {
            base.PostClose();
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        private void DrawHairColorSelector(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.3f, 0.3f, 0.3f));
            Rect contractedBy = rect.ContractedBy(MarginFS / 2);

            Rect set = new Rect(contractedBy);
            int colorFields = 7;
            int colorRows = 4;

            set.width = contractedBy.width / colorFields;
            set.height = Mathf.Min(set.width, contractedBy.height / (colorRows + 3));

            float euMelanin = this.faceComp.PawnFace.EuMelanin;

            int num = 0;
            for (int y = 0; y < colorRows; y++)
            {
                for (int x = 0; x < colorFields; x++)
                {
                    float pheoMelanin = (float)num / ((colorFields * colorRows) - 1);
                    HairColorRequest request = new HairColorRequest(
                        pheoMelanin,
                        euMelanin,
                        this.faceComp.PawnFace.Greyness);

                    this.DrawHairColorPickerCell(
                        HairMelanin.GetHairColor(request),
                        set.ContractedBy(2f),
                        "FacialStuffEditor.Pheomelanin".Translate() + " - " + pheoMelanin.ToString("N2"),
                        request);
                    set.x += set.width;
                    num++;
                }

                set.x = contractedBy.x;
                set.y += set.height;
            }

            set.y += set.height / 4;
            set.width = contractedBy.width / HairMelanin.ArtificialHairColors.Count;

            foreach (Color color in HairMelanin.ArtificialHairColors)
            {
                this.DrawHairColorPickerCell(color, set.ContractedBy(3f), color.ToString());
                set.x += set.width;
            }

            set.x = contractedBy.x;
            float col = contractedBy.width / 9;

            set.width = col * 4;
            set.y += 1.5f * set.height;

            // this.faceComp.PawnFace.PheoMelanin =
            // Widgets.HorizontalSlider(set, this.faceComp.PawnFace.PheoMelanin, 0f, 1f);
            // set.y += 30f;
            // this.faceComp.PawnFace.EuMelanin =
            // Widgets.HorizontalSlider(set, this.faceComp.PawnFace.EuMelanin, 0f, 1f);
            // set.y += 30f;
            euMelanin = Widgets.HorizontalSlider(
                set,
                euMelanin,
                0,
                1f,
                false,
                "FacialStuffEditor.Eumelanin".Translate(),
                "0",
                "1");

            set.x += set.width + col;

            float grey = this.faceComp.PawnFace.Greyness;
            grey = Widgets.HorizontalSlider(
                set,
                grey,
                HairMelanin.greyRange.min,
                HairMelanin.greyRange.max,
                false,
                "FacialStuffEditor.Greyness".Translate(),
                "0",
                "1");



            if (GUI.changed)
            {
                bool update = false;

                if (Math.Abs(this.faceComp.PawnFace.EuMelanin - euMelanin) > 0.001f)
                {
                    this.faceComp.PawnFace.EuMelanin = euMelanin;
                    update = true;
                }

                if (Math.Abs(this.faceComp.PawnFace.Greyness - grey) > 0.001f)
                {
                    this.faceComp.PawnFace.Greyness = grey;
                    update = true;
                }

                if (update)
                {
                    this.RemoveColorPicker();

                    this.NewHairColor = this.faceComp.PawnFace.GetCurrentHairColor();
                }
            }
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
                string path = this.faceComp.EyeTexPath(def.texPath, Side.Left);

                result = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                             path,
                             ShaderDatabase.CutoutSkin,
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

        [NotNull]
        private Graphic_Multi_NaturalHeadParts MoustacheGraphic([NotNull] MoustacheDef def)
        {
            string path = this.faceComp.GetMoustachePath(def);

            Graphic_Multi_NaturalHeadParts result = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                        path,
                                                        ShaderDatabase.Cutout,
                                                        new Vector2(38f, 38f),
                                                        Color.white,
                                                        Color.white) as Graphic_Multi_NaturalHeadParts;

            return result;
        }

        // ReSharper disable once MethodTooLong
        private void ResetPawnFace()
        {
            this.reInit = true;
            this.NewHairColor = this.originalHairColor;
            this.NewHair = this.originalHair;
            this.NewMelanin = this.originalMelanin;

            this.NewBeard = this.originalBeard;
            this.NewMoustache = this.originalMoustache;

            this.faceComp.PawnFace.HasSameBeardColor = this.hadSameBeardColor;
            this.NewBeardColor = this.originalBeardColor;

            this.NewEye = this.originalEye;
            this.NewBrow = this.originalBrow;

            pawn.story.bodyType = this.originalBodyType;
            pawn.gender = this.originalGender;
            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pawn.story, this.originalHeadGraphicPath);
            pawn.story.crownType = this.originalCrownType;
            pawn.ageTracker.AgeBiologicalTicks = this.originalAgeBio;
            pawn.ageTracker.AgeChronologicalTicks = this.originalAgeChrono;
            this.faceComp.PawnFace.wrinkles = this.wrinkles;

            this.reInit = false;
            this.rerenderPawn = true;
        }

        private Graphic_Multi_NaturalEyes RightEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes result;
            if (def.texPath != null)
            {
                string path = this.faceComp.EyeTexPath(def.texPath, Side.Right);

                // "Eyes/Eye_" + pawn.gender + faceComp.crownType + "_" + def.texPath   + "_Right";
                result = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                             path,
                             ShaderDatabase.CutoutSkin,
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

        private Action SetTabFaceStyle(FaceStyleTab tab)
        {
            return delegate { this.tab = tab; };
        }

        // ReSharper disable once MethodTooLong
        private void UpdatePawn(object sender, object value, object value2 = null)
        {
            if (sender == null)
            {
                return;
            }

            if (sender is BodyTypeSelectionDTO)
            {
                pawn.story.bodyType = (BodyType)value;
            }
            else if (sender is GenderSelectionDTO)
            {
                pawn.gender = (Gender)value;
            }
            else if (sender is HeadTypeSelectionDTO)
            {
                typeof(Pawn_StoryTracker).GetField(
                    "headGraphicPath",
                    BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, value);
                pawn.story.crownType = (CrownType)value2;
            }
            else if (sender is SliderWidgetDTO)
            {
                pawn.story.melanin = (float)value;
            }

            this.rerenderPawn = true;
        }

        private void UpdatePawnColors(object type, object newValue)
        {
            if (type == null)
            {
                return;
            }

            if (type is BeardDef)
            {
                this.faceComp.PawnFace.BeardColor = (Color)newValue;
            }

            if (type is HairDef)
            {
                pawn.story.hairColor = (Color)newValue;
                this.faceComp.PawnFace.HairColor = (Color)newValue;
            }

            // skin color
            if (type is Color)
            {
                pawn.story.melanin = (float)newValue;
            }

            this.rerenderPawn = true;
        }

        private void UpdatePawnDefs([NotNull] Def newValue)
        {
            if (newValue is BeardDef)
            {
                this.faceComp.PawnFace.BeardDef = (BeardDef)newValue;
            }

            if (newValue is MoustacheDef)
            {
                this.faceComp.PawnFace.MoustacheDef = (MoustacheDef)newValue;
            }

            if (newValue is EyeDef)
            {
                this.faceComp.PawnFace.EyeDef = (EyeDef)newValue;
            }

            if (newValue is BrowDef)
            {
                this.faceComp.PawnFace.BrowDef = (BrowDef)newValue;
            }

            if (newValue is HairDef)
            {
                pawn.story.hairDef = (HairDef)newValue;
            }

            this.rerenderPawn = true;
        }

        #endregion Private Methods

        // {
        // if (SelectedFacePreset != null && SelectedFacePreset.label.NullOrEmpty())
        // {
        // SelectedFacePreset.label = "Unnamed";
        // }
        // }
    }
}