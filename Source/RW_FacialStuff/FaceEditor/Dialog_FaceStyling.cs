using FacialStuff.DefOfs;
using FacialStuff.Defs;
using FacialStuff.FaceEditor.ColorPicker;
using FacialStuff.FaceEditor.UI.DTO;
using FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs;
using FacialStuff.FaceEditor.UI.Util;
using FacialStuff.Genetics;
using FacialStuff.GraphicsFS;
using FacialStuff.Harmony;
using FacialStuff.Utilities;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FacialStuff.FaceEditor
{

    // ReSharper disable once InconsistentNaming
    [StaticConstructorOnStartup]
    public partial class Dialog_FaceStyling : Window
    {
        #region Public Fields

        public static readonly Color ColorSwatchBorder = new Color(0.77255f, 0.77255f, 0.77255f);

        public static readonly Color ColorSwatchSelection = new Color(0.9098f, 0.9098f, 0.9098f);

        public static readonly int Columns;

        public static readonly float EntrySize;

        public static readonly float ListWidth = 450f;

        // private static Texture2D _icon;
        public static readonly float MarginFS = 14f;

        // private FacePreset _selFacePresetInt;
        public static readonly Texture2D NameBackground;

        public static readonly string Title = "FacialStuffEditor.FaceStylerTitle".Translate();

        public static readonly float TitleHeight = 30f;

        public static List<HairDef> FilteredHairDefs;

        [NotNull] public readonly CompFace CompFace;

        public readonly bool Initialized;
        public readonly Gender OriginalGender;
        public DresserDTO DresserDto;
        private FilterTab filterTab;
        public GenderTab genderTab;
        public Vector2 PickerPosition = Vector2.zero;
        public Vector2 PickerSize = new Vector2(200, 200);
        public bool RerenderPawn = true;

        public Vector2 ScrollPositionHairAll = Vector2.zero;

        public Vector2 ScrollPositionHairAny = Vector2.zero;

        public Vector2 ScrollPositionHairFemale = Vector2.zero;

        public Vector2 ScrollPositionHairMale = Vector2.zero;
        public bool SkinPage = true;
        public FaceStyleTab Tab;

#endregion Public Fields

#region Private Fields

        private static readonly Color DarkBackground = new Color(0.12f, 0.12f, 0.12f);

        public static List<BeardDef> FullBeardDefs;

        public static List<BeardDef> LowerBeardDefs;

        // private FacePreset SelectedFacePreset
        // {
        // get { return _selFacePresetInt; }
        // set
        // {
        // CheckSelectedFacePresetHasName();
        // _selFacePresetInt = value;
        // }
        // }
        public static List<MoustacheDef> MoustacheDefs;

        private static readonly long TicksPerYear = 3600000L;

        private static readonly List<string> VanillaHairTags = new List<string> { "Urban", "Rural", "Punk", "Tribal" };

        public static List<BrowDef> BrowDefs;

        private static List<string> _currentFilter = new List<string> { "Urban", "Rural", "Punk", "Tribal" };

        private static List<EyeDef> _eyeDefs;
        private static List<EarDef> _earDefs;

        private static List<HairDef> _hairDefs;

        public static Pawn Pawn;

        private static Vector2 _portraitSize = new Vector2(203f, 203f);

        private static float _previewSize = 220f;
        private readonly ColorWrapper _colourWrapper;

        private readonly bool _gear;

        private readonly bool _hadSameBeardColor;

        private readonly bool _hats;

        private readonly long _originalAgeBio;

        private readonly long _originalAgeChrono;

        private readonly BeardDef _originalBeard;

        private readonly Color _originalBeardColor;

        private readonly BodyTypeDef _originalBodyType;

        private readonly BrowDef _originalBrow;

        private readonly CrownType _originalCrownType;

        private readonly EyeDef _originalEye;
        private readonly EarDef _originalEar;

        private readonly HairDef _originalHair;

        private readonly Color _originalHairColor;

        private readonly string _originalHeadGraphicPath;

        private readonly float _originalMelanin;

        private readonly MoustacheDef _originalMoustache;

        private readonly float _wrinkles;

        private BeardTab _beardTab;

        private BeardDef _newBeard;

        private Color _newBeardColor;

        private BrowDef _newBrow;

        private EyeDef _newEye;
        private EarDef _newEar;

        private HairDef _newHair;

        private Color _newHairColor;

        private float _newMelanin;

        private MoustacheDef _newMoustache;

        [SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "Reviewed. Suppression is OK here.")]
        private bool _reInit;

        private bool _saveChangedOnExit;

        private Vector2 _scrollPositionBeard1 = Vector2.zero;

        private Vector2 _scrollPositionBeard2 = Vector2.zero;

        private Vector2 _scrollPositionBrow = Vector2.zero;

        private Vector2 _scrollPositionEye = Vector2.zero;

#if legacy
        private SpecialTab _specialTab;
#endif
        private Vector2 _swatchSize = new Vector2(14, 14);
        public PawnFace PawnFace => this._pawnFace;

#endregion Private Fields

#region Public Constructors
        static Dialog_FaceStyling()
        {
            // _previewSize = 100f;

            // _icon = ContentFinder<Texture2D>.Get("ClothIcon");

            // _listWidth = 200f;
            Columns = 12;
            EntrySize = ListWidth / Columns;
            NameBackground = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));
            HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                x => x.hairTags
                                                                                      .SharesElementWith(VanillaHairTags) && !x.IsBeardNotHair());

            _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading;
            _earDefs = DefDatabase<EarDef>.AllDefsListForReading;
            FullBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType == BeardType.FullBeard)
                                                 .ToList();
            LowerBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType != BeardType.FullBeard)
                                                  .ToList();
            MoustacheDefs = DefDatabase<MoustacheDef>.AllDefsListForReading;

            BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading;

            //todo this piece was totally broken and I've got no idea if it'll work
            FullBeardDefs.SortBy(i => i.LabelCap.ToString());
            LowerBeardDefs.SortBy(i => i.LabelCap.ToString());
            MoustacheDefs.SortBy(i => i.LabelCap.ToString());
            HairDefs.SortBy(i => i.LabelCap.ToString());
        }

        public Dialog_FaceStyling(CompFace face)
        {
            this.CompFace = face;
            Pawn = this.CompFace.Pawn;

            this._hats = Prefs.HatsOnlyOnMap;
            this._gear = Controller.settings.FilterHats;
            Prefs.HatsOnlyOnMap = true;
            Controller.settings.FilterHats = false;
            this._pawnFace = this.CompFace.PawnFace;

            if (this.PawnFace == null)
            {
                return;
            }

            this._hadSameBeardColor = this.PawnFace.HasSameBeardColor;

            if (Pawn.gender == Gender.Female)
            {
                this.genderTab = GenderTab.Female;
            }
            else
            {
                this.genderTab = GenderTab.Male;
            }

            CurrentFilter = Pawn.story.hairDef.hairTags;
            if (Pawn.story.hairDef.hairTags.Contains("Urban"))
            {
                this.filterTab = FilterTab.Urban;
            }
            else if (Pawn.story.hairDef.hairTags.Contains("Rural"))
            {
                this.filterTab = FilterTab.Rural;
            }
            else if (Pawn.story.hairDef.hairTags.Contains("Punk"))
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
            // IncidentDef def = IncidentDef.Named("FacialStuffUpdateNote");
            // StorytellerComp source = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_ThreatCycle || x is StorytellerComp_RandomMain);
            // IncidentParms parms = source.GenerateParms(def.category, target);
            // FiringIncident fi = new FiringIncident(def, source, parms);
            // if (fi.def.Worker.CanFireNow(pawn.Map))
            // {
            // fi.def.Worker.TryExecute(parms);
            // fi.parms.target.StoryState.Notify_IncidentFired(fi);
            // }
            // }
            this._beardTab = this.PawnFace.BeardDef.beardType == BeardType.FullBeard
                        ? BeardTab.FullBeards
                        : BeardTab.Combinable;

            this._colourWrapper = new ColorWrapper(Color.cyan);
            this._newHairColor = this._originalHairColor = Pawn.story.hairColor;
            this._newBeardColor = this._originalBeardColor = this.PawnFace.BeardColor;
            this._newBeard = this._originalBeard = this.PawnFace.BeardDef;
            this._newMoustache = this._originalMoustache = this.PawnFace.MoustacheDef;
            this._newEye = this._originalEye = this.PawnFace.EyeDef;
            this._newBrow = this._originalBrow = this.PawnFace.BrowDef;
            this._newMelanin = this._originalMelanin = Pawn.story.melanin;
            this._newHair = this._originalHair = Pawn.story.hairDef;
            this._originalBodyType = Pawn.story.bodyType;
            this.OriginalGender = Pawn.gender;
            this._originalHeadGraphicPath = Pawn.story.HeadGraphicPath;
            this._originalCrownType = Pawn.story.crownType;
            this._originalAgeBio = Pawn.ageTracker.AgeBiologicalTicks;
            this._originalAgeChrono = Pawn.ageTracker.AgeChronologicalTicks;
            this._wrinkles = this.PawnFace.WrinkleIntensity;

            // this.absorbInputAroundWindow = false;
            this.closeOnClickedOutside = false;
            this.closeOnCancel = true;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.RerenderPawn = true;
        }

#endregion Public Constructors

#region Private Enums

#endregion Private Enums

#region Public Properties

        public static List<string> CurrentFilter
        {
            get => _currentFilter;

            set
            {
                _currentFilter = value;
                FilteredHairDefs = _hairDefs.FindAll(x => x.hairTags.SharesElementWith(_currentFilter));
                FilteredHairDefs.SortBy(i => i.LabelCap.ToString());    //Check it
            }
        }

        public static List<HairDef> HairDefs
        {
            get => _hairDefs;

            set
            {
                _hairDefs = value;
                FilteredHairDefs = _hairDefs.FindAll(x => x.hairTags.SharesElementWith(CurrentFilter));
                FilteredHairDefs.SortBy(i => i.LabelCap.ToString());
            }
        }

        public override Vector2 InitialSize => new Vector2(750f, 700f);

        public Color NewBeardColor
        {
            get => this._newBeardColor;

            set
            {
                this._newBeardColor = value;
                this.UpdatePawnColors(this.NewBeard, value);
            }
        }

        public Color NewHairColor
        {
            get => this._newHairColor;

            set
            {
                this._newHairColor = value;
                this.UpdatePawnColors(this.NewHair, value);

                if (this.PawnFace.HasSameBeardColor && !this._reInit)
                {
                    Color color = HairMelanin.ShuffledBeardColor(value);
                    this.UpdatePawnColors(this.NewBeard, color);
                }
            }
        }

        public float NewMelanin
        {
            get => Pawn.story.melanin;

            set
            {
                this._newMelanin = value;
                this.UpdatePawnColors(Color.green, value);
            }
        }

        public MoustacheDef NewMoustache
        {
            get => this._newMoustache;

            set
            {
                this._newMoustache = value;
                this.UpdatePawnDefs(value);

                if (this._newBeard.beardType == BeardType.FullBeard && !this._reInit)
                {
                    this._newBeard = PawnFaceMaker.RandomBeardDefFor(Pawn, BeardType.LowerBeard);
                    this.UpdatePawnDefs(this._newBeard);
                }
            }
        }

#endregion Public Properties

#region Private Properties

        private BeardDef NewBeard
        {
            get => this._newBeard;

            set
            {
                this._newBeard = value;

                this.UpdatePawnDefs(value);
                if (value.beardType == BeardType.FullBeard && !this._reInit)
                {
                    this._newMoustache = MoustacheDefOf.Shaved;
                    this.UpdatePawnDefs(MoustacheDefOf.Shaved);
                }
            }
        }

        private BrowDef NewBrow
        {
            get => this._newBrow;

            set
            {
                this._newBrow = value;
                this.UpdatePawnDefs(value);
            }
        }

        private EyeDef NewEye
        {
            get => this._newEye;

            set
            {
                this._newEye = value;

                this.UpdatePawnDefs(value);
            }
        }
        private EarDef NewEar
        {
            get => this._newEar;

            set
            {
                this._newEar = value;

                this.UpdatePawnDefs(value);
            }
        }

        private HairDef NewHair
        {
            get => this._newHair;

            set
            {
                this._newHair = value;
                this.UpdatePawnDefs(value);
            }
        }

        private readonly PawnFace _pawnFace;

#endregion Private Properties

#region Public Methods

        public static Rect AddPortraitWidget(float left, float top)
        {
            // Portrait
            Rect rect = new Rect(left, top, _portraitSize.x, _portraitSize.y);
            GUI.DrawTexture(rect, FaceTextures.BackgroundTex);

            // Draw the pawn's portrait
            GUI.BeginGroup(rect);
            Vector2 size = new Vector2(rect.height / 1.4f, rect.height); // 128x180
            Rect position = new Rect(
                                        rect.width * 0.5f - size.x * 0.5f,
                                        10f + rect.height * 0.5f - size.y * 0.5f,
                                        size.x,
                                        size.y);
            GUI.DrawTexture(position, PortraitsCache.Get(Pawn, size, new Vector3(0f, 0f, 0.1f), 1.25f));

            // GUI.DrawTexture(position, PortraitsCache.Get(pawn, size, default(Vector3)));
            GUI.EndGroup();

            GUI.color = Color.white;
            Widgets.DrawBox(rect);

            return rect;
        }

        public void DoColorWindowBeard()
        {
            this.RemoveColorPicker();

            this._colourWrapper.Color = this.PawnFace.HasSameBeardColor
                                   ? this.NewHairColor
                                   : this.NewBeardColor;
            Find.WindowStack.Add(
                                 new Dialog_ColorPicker(this._colourWrapper,
                                                        delegate
                                                        {
                                                            if (this.PawnFace.HasSameBeardColor)
                                                            {
                                                                this.NewHairColor = this._colourWrapper.Color;
                                                            }

                                                            this.NewBeardColor = this._colourWrapper.Color;
                                                        },
                                                        false,
                                                        true)
                                 {
                                     initialPosition =
                                 new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                                 });
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
                if (this.RerenderPawn)
                {
                    Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(Pawn);
                    this.RerenderPawn = false;
                }

                if (!this.Initialized)
                {
                    this.DresserDto = new DresserDTO(Pawn);
                    this.DresserDto.SetUpdatePawnListeners(this.UpdatePawn);
                }
            }
            catch (Exception ex)
            {
            }

            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                                           "FacialStuffEditor.Hair".Translate(), this.SetTabFaceStyle(FaceStyleTab.Hair), this.Tab == FaceStyleTab.Hair);
            list.Add(item);

            if (this.CompFace.Props.hasBeard)
            {
                if (Pawn.gender == Gender.Male)
                {
                    TabRecord item2 = new TabRecord(
                                                    "FacialStuffEditor.Beard".Translate(), this.SetTabFaceStyle(FaceStyleTab.Beard), this.Tab == FaceStyleTab.Beard);
                    list.Add(item2);
                }
            }

            if (this.CompFace.Props.hasEyes)
            {
                TabRecord item3 = new TabRecord(
                                                "FacialStuffEditor.Eye".Translate(), this.SetTabFaceStyle(FaceStyleTab.Eye), this.Tab == FaceStyleTab.Eye);
                list.Add(item3);

                TabRecord item4 = new TabRecord(
                                                "FacialStuffEditor.Brow".Translate(), this.SetTabFaceStyle(FaceStyleTab.Brow), this.Tab == FaceStyleTab.Brow);
                list.Add(item4);
            }

            if (Controller.settings.ShowBodyChange && Current.ProgramState == ProgramState.Playing)
            {
                TabRecord item5 = new TabRecord(
                                                "FacialStuffEditor.TypeSelector".Translate(), this.SetTabFaceStyle(FaceStyleTab.TypeSelector), this.Tab == FaceStyleTab.TypeSelector);
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
                if (this.OriginalGender != Gender.Male && this.Tab == FaceStyleTab.Beard)
                {
                    this.Tab = FaceStyleTab.Hair;
                }

                this.ResetPawnFace();
            };

            DialogUtility.DoNextBackButtons(
                                            inRect,
                                            "Randomize".Translate(),
                                            "FacialStuffEditor.Accept".Translate(),
                                            backAct, this.FaceRandomizer, this.SaveAndClose);
        }

        public void DrawBeardPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();

            void FullBeards()
            {
                // HairDefs = hairDefs.FindAll(x => x.hairTags.SharesElementWith(VanillaHairTags) &&
                //                                  (x.hairGender == HairGender.Female ||
                //                                   x.hairGender == HairGender.FemaleUsually));
                // HairDefs.SortBy(i => i.LabelCap);
                this._beardTab = BeardTab.FullBeards;
            }

            TabRecord item = new TabRecord(
                                           "FacialStuffEditor.FullBeards".Translate(),
                                           FullBeards, this._beardTab == BeardTab.FullBeards);
            list.Add(item);

            void CombinableBeards()
            {
                // HairDefs = hairDefs.FindAll(x => x.hairTags.SharesElementWith(VanillaHairTags) && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                // HairDefs.SortBy(i => i.LabelCap);
                this._beardTab = BeardTab.Combinable;
            }

            TabRecord item2 = new TabRecord(
                                            "FacialStuffEditor.Combinable".Translate(),
                                            CombinableBeards, this._beardTab == BeardTab.Combinable);
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

            if (this._beardTab == BeardTab.Combinable)
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

            switch (this._beardTab)
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
            if (this._beardTab == BeardTab.Combinable)
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

            if (this._beardTab == BeardTab.FullBeards)
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

        public void DrawBrowPicker(Rect rect)
        {
            // 12 columns as base
            int divider = 3;
            int iconSides = 1;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(BrowDefs.Count / (float)thisColumns);

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
            Widgets.BeginScrollView(rect2, ref this._scrollPositionBrow, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < BrowDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                this.DrawBrowPickerCell(BrowDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        public void DrawColorSelected(Rect swatchRect)
        {
            Widgets.DrawBoxSolid(swatchRect, Color.white);
        }

        public void DrawEyePicker(Rect rect)
        {
            // 12 columns as base
            int divider = 3;
            int iconSides = 1;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(_eyeDefs.Count / (float)thisColumns);

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
            Widgets.BeginScrollView(rect2, ref this._scrollPositionEye, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < _eyeDefs.Count; i++)
            {
                int num2 = i / thisColumns;
                int num3 = i % thisColumns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawEyePickerCell(_eyeDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        public void DrawHairColorPickerCell(
        Color color,
        Rect rect,
        string colorName,
        [CanBeNull] HairColorRequest colorRequest = null)
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

                if (colorRequest != null)
                {
                    this.PawnFace.PheoMelanin = colorRequest.PheoMelanin;
                    this.PawnFace.EuMelanin = colorRequest.EuMelanin;
                }

                this._colourWrapper.Color = color;
                Find.WindowStack.Add(
                                     new Dialog_ColorPicker(this._colourWrapper,
                                                            delegate { this.NewHairColor = this._colourWrapper.Color; },
                                                            false,
                                                            true)
                                     {
                                         initialPosition =
                                     new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                                     });
            }
        }

        public virtual void DrawHairPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                                           "Female".Translate(),
                                           delegate
                                           {
                                               HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                                             x =>
                                                                                                                 x.hairTags
                                                                                                                  .SharesElementWith(VanillaHairTags)
                                                                                                               &&
                                                                                                                 (x.hairGender ==
                                                                                                                  HairGender
                                                                                                                 .Female ||
                                                                                                                  x.hairGender ==
                                                                                                                  HairGender
                                                                                                                 .FemaleUsually && !x.IsBeardNotHair()
                                                                                                                 ));
                                               HairDefs.SortBy(i => i.LabelCap.ToString());
                                               this.genderTab = GenderTab.Female;
                                           }, this.genderTab == GenderTab.Female);
            list.Add(item);

            TabRecord item2 = new TabRecord(
                                            "Male".Translate(),
                                            delegate
                                            {
                                                HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                                              x =>
                                                                                                                  x.hairTags
                                                                                                                   .SharesElementWith(VanillaHairTags)
                                                                                                                &&
                                                                                                                  (x.hairGender ==
                                                                                                                   HairGender
                                                                                                                  .Male ||
                                                                                                                   x.hairGender ==
                                                                                                                   HairGender
                                                                                                                  .MaleUsually && !x.IsBeardNotHair()
                                                                                                                  ));
                                                HairDefs.SortBy(i => i.LabelCap.ToString());
                                                this.genderTab = GenderTab.Male;
                                            }, this.genderTab == GenderTab.Male);
            list.Add(item2);

            TabRecord item3 = new TabRecord(
                                            "FacialStuffEditor.Any".Translate(),
                                            delegate
                                            {
                                                HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                                              x =>
                                                                                                                  x.hairTags
                                                                                                                   .SharesElementWith(VanillaHairTags) &&
                                                                                                                  x.hairGender ==
                                                                                                                  HairGender
                                                                                                                 .Any && !x.IsBeardNotHair());
                                                HairDefs.SortBy(i => i.LabelCap.ToString());
                                                this.genderTab = GenderTab.Any;
                                            }, this.genderTab == GenderTab.Any);
            list.Add(item3);

            TabRecord item4 = new TabRecord(
                                            "FacialStuffEditor.All".Translate(),
                                            delegate
                                            {
                                                HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                                              x => x
                                                                                                                  .hairTags
                                                                                                                  .SharesElementWith(VanillaHairTags) && !x.IsBeardNotHair());
                                                HairDefs.SortBy(i => i.LabelCap.ToString());
                                                this.genderTab = GenderTab.All;
                                            }, this.genderTab == GenderTab.All);

            list.Add(item4);

            TabDrawer.DrawTabs(rect, list);

            List<TabRecord> list2 = new List<TabRecord>();

            TabRecord urban = new TabRecord(
                                            "FacialStuffEditor.Urban".Translate(),
                                            delegate
                                            {
                                                CurrentFilter = new List<string> { "Urban" };
                                                this.filterTab = FilterTab.Urban;
                                            }, this.filterTab == FilterTab.Urban);
            list2.Add(urban);

            TabRecord rural = new TabRecord(
                                            "FacialStuffEditor.Rural".Translate(),
                                            delegate
                                            {
                                                CurrentFilter = new List<string> { "Rural" };
                                                this.filterTab = FilterTab.Rural;
                                            }, this.filterTab == FilterTab.Rural);
            list2.Add(rural);

            TabRecord punk = new TabRecord(
                                           "FacialStuffEditor.Punk".Translate(),
                                           delegate
                                           {
                                               CurrentFilter = new List<string> { "Punk" };
                                               this.filterTab = FilterTab.Punk;
                                           }, this.filterTab == FilterTab.Punk);
            list2.Add(punk);

            TabRecord tribal = new TabRecord(
                                             "FacialStuffEditor.Tribal".Translate(),
                                             delegate
                                             {
                                                 CurrentFilter = new List<string> { "Tribal" };
                                                 this.filterTab = FilterTab.Tribal;
                                             }, this.filterTab == FilterTab.Tribal);
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
                    Widgets.BeginScrollView(rect2, ref this.ScrollPositionHairMale, rect3);
                    break;

                case GenderTab.Female:
                    Widgets.BeginScrollView(rect2, ref this.ScrollPositionHairFemale, rect3);
                    break;

                case GenderTab.Any:
                    Widgets.BeginScrollView(rect2, ref this.ScrollPositionHairAny, rect3);
                    break;

                case GenderTab.All:
                    Widgets.BeginScrollView(rect2, ref this.ScrollPositionHairAll, rect3);
                    break;
            }

            GUI.BeginGroup(rect3);

            for (int i = 0; i < FilteredHairDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                this.DrawHairPickerCell(FilteredHairDefs[i], rect4.ContractedBy(3f));
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
                if (hair == this.NewHair)
                {
                    Widgets.DrawHighlightSelected(rect);
                    label += "\n(selected)";
                }
                else
                {
                    if (hair == this._originalHair)
                    {
                        Widgets.DrawAltRect(rect);
                        label += "\n(original)";
                    }
                }
            }

            string highlightText = string.Empty;
            foreach (string hairTag in hair.hairTags)
            {
                if (!highlightText.NullOrEmpty())
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
            GUI.DrawTexture(rect1, this.HairGraphic(hair).MatSouth.mainTexture);
            GUI.DrawTexture(rect2, this.HairGraphic(hair).MatEast.mainTexture);

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
                    this._colourWrapper.Color = this.NewHairColor;

                    Find.WindowStack.Add(
                                         new Dialog_ColorPicker(this._colourWrapper,
                                                                delegate { this.NewHairColor = this._colourWrapper.Color; },
                                                                false,
                                                                true)
                                         {
                                             initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                                         });
                }
            }
        }

        public void DrawTypeSelector(Rect rect)
        {
            float editorLeft = rect.x;
            float editorTop = 30f + WidgetUtil.SelectionRowHeight;
            const float editorWidth = 325f;

            float top = editorTop + 64f;

            BodyTypeSelectionDto dresserDtoBodyTypeSelectionDto = this.DresserDto.BodyTypeSelectionDto;
            if (dresserDtoBodyTypeSelectionDto != null)
            {
                WidgetUtil.AddSelectorWidget(
                    editorLeft,
                    top,
                    editorWidth,
                    "FacialStuffEditor.BodyType".Translate() + ":", dresserDtoBodyTypeSelectionDto);
            }

            top += WidgetUtil.SelectionRowHeight + 20f;
            HeadTypeSelectionDto dresserDtoHeadTypeSelectionDto = this.DresserDto.HeadTypeSelectionDto;
            if (dresserDtoHeadTypeSelectionDto != null)
                WidgetUtil.AddSelectorWidget(
                    editorLeft,
                    top,
                    editorWidth,
                    "FacialStuffEditor.HeadType".Translate() + ":", dresserDtoHeadTypeSelectionDto);

            top += WidgetUtil.SelectionRowHeight + 20f;

            if (Controller.settings.ShowGenderAgeChange)
            {
                GUI.Label(
                          new Rect(editorLeft, top, editorWidth, 64f),
                          "FacialStuffEditor.GenderChangeWarning".Translate());

                top += 64f + 20f;

                GenderSelectionDto dresserDtoGenderSelectionDto = this.DresserDto.GenderSelectionDto;
                if (dresserDtoGenderSelectionDto != null)
                    WidgetUtil.AddSelectorWidget(
                        editorLeft,
                        top,
                        editorWidth,
                        "FacialStuffEditor.Gender".Translate() + ":", dresserDtoGenderSelectionDto);

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

        public virtual void DrawUI(Rect rect)
        {
            GUI.BeginGroup(rect);
            string pawnName = Pawn.LabelShort;
            Vector2 vector = Text.CalcSize(pawnName);

            Rect pawnRect = AddPortraitWidget(0f, TitleHeight);
            Rect labelRect = new Rect(0f, pawnRect.yMax, vector.x, vector.y);
            labelRect = labelRect.CenteredOnXIn(pawnRect);

            float width = rect.width - ListWidth - MarginFS;

            Rect button = new Rect(
                                   0f,
                                   labelRect.yMax + MarginFS / 2,
                                   (width - MarginFS) / 2,
                                   WidgetUtil.SelectionRowHeight);
            Rect mainRect = new Rect(0f, button.yMax + MarginFS, width, 65f);
            if (Widgets.ButtonText(button, "FacialStuffEditor.SkinSettings".Translate()))
            {
                this.RemoveColorPicker();

                this.SkinPage = true;
            }

            button.x = button.xMax + MarginFS;

            if (Widgets.ButtonText(button, "FacialStuffEditor.HairSettings".Translate()))
            {
                if (this.Tab == FaceStyleTab.Beard)
                {
                    this.DoColorWindowBeard();
                }

                this.SkinPage = false;
            }

            float height = rect.height - MarginFS * 3 - TitleHeight;

            Rect listRect = new Rect(0f, TitleHeight, ListWidth, height) { x = mainRect.xMax + MarginFS };

            mainRect.yMax = listRect.yMax;

            this.PickerPosition = new Vector2(mainRect.position.x, mainRect.position.y);
            this.PickerSize = new Vector2(mainRect.width, mainRect.height);

            GUI.DrawTexture(
                            new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height),
                            NameBackground);
            Widgets.Label(labelRect, pawnName);

            Rect set = new Rect(mainRect) { height = WidgetUtil.SelectionRowHeight, width = mainRect.width / 2 - 10f };
            set.y = listRect.yMax -
                       WidgetUtil.SelectionRowHeight;
            set.width = mainRect.width -
                        MarginFS / 3;

            bool faceCompDrawMouth = this.PawnFace.DrawMouth;
            bool faceCompHasSameBeardColor = this.PawnFace.HasSameBeardColor;

            mainRect.yMax -= WidgetUtil.SelectionRowHeight + MarginFS;
            if (this.SkinPage)
            {
                this.DrawSkinColorSelector(mainRect);
                if (Controller.settings.UseMouth)
                {
                    Widgets.CheckboxLabeled(set, "FacialStuffEditor.DrawMouth".Translate(), ref faceCompDrawMouth);
                }
            }
            else
            {
                if (this.Tab == FaceStyleTab.Beard && !faceCompHasSameBeardColor)
                {
                }
                else
                {
                    this.DrawHairColorSelector(mainRect);
                }

                if (Pawn.gender == Gender.Male)
                {
                    Widgets.CheckboxLabeled(
                                            set,
                                            "FacialStuffEditor.SameColor".Translate(),
                                            ref faceCompHasSameBeardColor);
                    TooltipHandler.TipRegion(set, "FacialStuffEditor.SameColorTip".Translate());
                }
            }

            if (this.Tab == FaceStyleTab.Hair || this.Tab == FaceStyleTab.Beard)
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
                if (this.PawnFace.HasSameBeardColor != faceCompHasSameBeardColor)
                {
                    this.RemoveColorPicker();
                    this.PawnFace.HasSameBeardColor = faceCompHasSameBeardColor;
                    this.NewBeardColor = HairMelanin.ShuffledBeardColor(this.NewHairColor);
                }

                if (this.PawnFace.DrawMouth != faceCompDrawMouth)
                {
                    this.PawnFace.DrawMouth = faceCompDrawMouth;
                    this.RerenderPawn = true;
                }
            }

            set.width = mainRect.width / 2 - 10f;

            set.y += 36f;
            set.x = mainRect.x;

            if (this.Tab == FaceStyleTab.Eye)
            {
                this.DrawEyePicker(listRect);
            }

            if (this.Tab == FaceStyleTab.Brow)
            {
                if (Pawn.gender == Gender.Female)
                {
                    BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                                                                                  x =>
                                                                                      x.hairGender ==
                                                                                      HairGender.Female ||
                                                                                      x.hairGender ==
                                                                                      HairGender.FemaleUsually);
                    BrowDefs.SortBy(i => i.LabelCap.ToString());
                }
                else
                {
                    BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                                                                                  x =>
                                                                                      x.hairGender == HairGender.Male ||
                                                                                      x.hairGender ==
                                                                                      HairGender.MaleUsually);
                    BrowDefs.SortBy(i => i.LabelCap.ToString());
                }

                this.DrawBrowPicker(listRect);
            }

            if (this.Tab == FaceStyleTab.Hair)
            {
                this.DrawHairPicker(listRect);
            }

            if (this.Tab == FaceStyleTab.Beard)
            {
                this.DrawBeardPicker(listRect);
            }

            if (this.Tab == FaceStyleTab.TypeSelector)
            {
                this.DrawTypeSelector(listRect);
            }

            GUI.EndGroup();
        }

        public void FaceRandomizer()
        {
            while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
            {
            }

            // HairDNA hair = HairMelanin.GenerateHairMelaninAndCuticula(pawn, Rand.Value > 0.5f);
            this._reInit = true;
            this.PawnFace.HasSameBeardColor = Rand.Value > 0.3f;
            this.NewHair = PawnHairChooser.RandomHairDefFor(Pawn, Faction.OfPlayer.def);
            // Pawn.story.melanin = PawnSkinColors.RandomMelanin(Pawn.Faction);
            this.PawnFace.GenerateHairDNA(Pawn, true);
            this.NewHairColor = this.PawnFace.HairColor;
            this.NewBeardColor = this.PawnFace.BeardColor;
            PawnFaceMaker.RandomBeardDefFor(this.CompFace,
                                            Faction.OfPlayer.def,
                                            out BeardDef beard,
                                            out MoustacheDef tache);
            this.NewBeard = beard;
            this.NewMoustache = tache;

            this.NewEye = PawnFaceMaker.RandomEyeDefFor(Pawn,
                Faction.OfPlayer.def);
            this.NewEar = PawnFaceMaker.RandomEarDefFor(Pawn,
                Faction.OfPlayer.def);
            this.NewBrow = PawnFaceMaker.RandomBrowDefFor(Pawn, Faction.OfPlayer.def);

            this._reInit = false;

            // SoundDef.Named("ShotPistol").PlayOneShot();
        }

        public override void PostClose()
        {
            base.PostClose();
            Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public override void PostOpen()
        {
            FillDefs();
            _hairDefs.SortBy(i => i.LabelCap.ToString());
            _eyeDefs.SortBy(i => i.LabelCap.ToString());
            BrowDefs.SortBy(i => i.LabelCap.ToString());
        }

        public override void PreClose()
        {
            Prefs.HatsOnlyOnMap = this._hats;
            Controller.settings.FilterHats = this._gear;
            if (!this._saveChangedOnExit)
            {
                this.ResetPawnFace();
            }

            this.RemoveColorPicker();
        }

        public void RemoveColorPicker()
        {
            while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker), false))
            {
            }
        }

        // ReSharper disable once MethodTooLong
        public void ResetPawnFace()
        {
            this._reInit = true;
            this.NewHairColor = this._originalHairColor;
            this.NewHair = this._originalHair;
            this.NewMelanin = this._originalMelanin;

            this.NewBeard = this._originalBeard;
            this.NewMoustache = this._originalMoustache;

            this.PawnFace.HasSameBeardColor = this._hadSameBeardColor;
            this.NewBeardColor = this._originalBeardColor;

            this.NewEye = this._originalEye;
            this.NewBrow = this._originalBrow;

            Pawn.story.bodyType = this._originalBodyType;
            Pawn.gender = this.OriginalGender;
            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.SetValue(Pawn.story, this._originalHeadGraphicPath);
            Pawn.story.crownType = this._originalCrownType;
            Pawn.ageTracker.AgeBiologicalTicks = this._originalAgeBio;
            Pawn.ageTracker.AgeChronologicalTicks = this._originalAgeChrono;
            this.PawnFace.WrinkleIntensity = this._wrinkles;

            this._reInit = false;
            this.RerenderPawn = true;
        }

        public void SaveAndClose()
        {
            this._saveChangedOnExit = true;

            // SoundDef.Named("ShotShotgun").PlayOneShotOnCamera();
            this.Close();
        }

        public Action SetTabFaceStyle(FaceStyleTab tab)
        {
            return delegate { this.Tab = tab; };
        }

        // ReSharper disable once MethodTooLong
        public void UpdatePawn(object sender, object value, object value2 = null)
        {
            if (sender == null)
            {
                return;
            }

            if (sender is BodyTypeSelectionDto)
            {
                Pawn.story.bodyType = (BodyTypeDef)value;
            }
            else if (sender is GenderSelectionDto)
            {
                Pawn.gender = (Gender)value;
            }
            else if (sender is HeadTypeSelectionDto)
            {
                typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic)
                                        ?.SetValue(Pawn.story, value);
                if (value2 != null)
                {
                    Pawn.story.crownType = (CrownType)value2;
                }
            }
            else if (sender is SliderWidgetDto)
            {
                Pawn.story.melanin = (float)value;
            }

            this.RerenderPawn = true;
        }

#endregion Public Methods

#region Private Methods

        // ReSharper disable once MethodTooLong
        private static void FillDefs()
        {
            switch (Pawn.gender)
            {
                default:
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                  x =>
                                                                                      x.hairTags
                                                                                       .SharesElementWith(VanillaHairTags)
                                                                                   && (x.hairGender ==
                                                                                       HairGender.Male ||
                                                                                       x.hairGender ==
                                                                                       HairGender.MaleUsually && !x.IsBeardNotHair()));
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                                                                                 x => x.hairGender == HairGender.Male ||
                                                                                      x.hairGender ==
                                                                                      HairGender.MaleUsually);
                    _earDefs = DefDatabase<EarDef>.AllDefsListForReading.FindAll(
                                                                                 x => x.hairGender == HairGender.Male ||
                                                                                      x.hairGender ==
                                                                                      HairGender.MaleUsually);
                    BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                                                                                  x =>
                                                                                      x.hairGender == HairGender.Male ||
                                                                                      x.hairGender ==
                                                                                      HairGender.MaleUsually);
                    break;

                case Gender.Female:
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                  x =>
                                                                                      x.hairTags
                                                                                       .SharesElementWith(VanillaHairTags)
                                                                                   && (x.hairGender ==
                                                                                       HairGender.Female ||
                                                                                       x.hairGender ==
                                                                                       HairGender.FemaleUsually && !x.IsBeardNotHair()));
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                                                                                 x =>
                                                                                     x.hairGender ==
                                                                                     HairGender.Female ||
                                                                                     x.hairGender ==
                                                                                     HairGender.FemaleUsually);
                    _earDefs = DefDatabase<EarDef>.AllDefsListForReading.FindAll(
                                                                                 x =>
                                                                                     x.hairGender ==
                                                                                     HairGender.Female ||
                                                                                     x.hairGender ==
                                                                                     HairGender.FemaleUsually);
                    BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                                                                                  x =>
                                                                                      x.hairGender ==
                                                                                      HairGender.Female ||
                                                                                      x.hairGender ==
                                                                                      HairGender.FemaleUsually);
                    break;
            }
        }

        [CanBeNull]
        private Graphic_Multi_NaturalHeadParts BeardGraphic([NotNull] BeardDef def)
        {
            string path = this.CompFace.GetBeardPath(def);

            Graphic_Multi_NaturalHeadParts graphic =
            GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                path,
                                                                ShaderDatabase.Cutout,
                                                                new Vector2(38f, 38f),
                                                                Color.white,
                                                                Color.white) as Graphic_Multi_NaturalHeadParts;

            return graphic;
        }

        private Graphic_Multi_NaturalHeadParts BrowGraphic(BrowDef def)
        {
            Graphic_Multi_NaturalHeadParts __result =
            GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(this.CompFace.BrowTexPath(def),
                                                                ShaderDatabase.CutoutSkin,
                                                                new Vector2(38f, 38f),
                                                                Color.white,
                                                                Color.white) as
            Graphic_Multi_NaturalHeadParts;



            return __result;
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

                this._colourWrapper.Color = color;
                Find.WindowStack.Add(
                                     new Dialog_ColorPicker(this._colourWrapper,
                                                            delegate
                                                            {
                                                                if (this.PawnFace.HasSameBeardColor)
                                                                {
                                                                    this.NewHairColor = this._colourWrapper.Color;
                                                                }

                                                                this.NewBeardColor = this._colourWrapper.Color;
                                                            },
                                                            false,
                                                            true)
                                     {
                                         initialPosition =
                                     new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                                     });
            }
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
                    if (beard == this._originalBeard)
                    {
                        Widgets.DrawAltRect(rect);
                        text += "\n(original)";
                    }
                }
            }

            if (this._newMoustache != MoustacheDefOf.Shaved)
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
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewBeard = beard;
                this.DoColorWindowBeard();
            }
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
                if (brow == this._originalBrow)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.color = Color.black;
            GUI.DrawTexture(rect, this.BrowGraphic(brow).MatSouth.mainTexture);
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
                if (eye == this._originalEye)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.DrawTexture(rect, this.RightEyeGraphic(eye).MatSouth.mainTexture);
            GUI.DrawTexture(rect, this.LeftEyeGraphic(eye).MatSouth.mainTexture);

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

        private void DrawEarPickerCell(EarDef ear, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = ear.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (ear == this.NewEar)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (ear == this._originalEar)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.DrawTexture(rect, this.RightEarGraphic(ear).MatSouth.mainTexture);
            GUI.DrawTexture(rect, this.LeftEarGraphic(ear).MatSouth.mainTexture);

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewEar = ear;
                this.RemoveColorPicker();
            }
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

            float euMelanin = this.PawnFace.EuMelanin;

            int num = 0;
            for (int y = 0; y < colorRows; y++)
            {
                for (int x = 0; x < colorFields; x++)
                {
                    float pheoMelanin = (float)num / (colorFields * colorRows - 1);
                    HairColorRequest colorRequest =
                    new HairColorRequest(pheoMelanin, euMelanin, this.PawnFace.Greyness);

                    this.DrawHairColorPickerCell(
                                            HairMelanin.GetHairColor(colorRequest),
                                            set.ContractedBy(2f),
                                            "FacialStuffEditor.Pheomelanin".Translate() + " - " +
                                            pheoMelanin.ToString("N2"),
                                            colorRequest);
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

            // this.compFace.PawnFace.PheoMelanin =
            // Widgets.HorizontalSlider(set, this.compFace.PawnFace.PheoMelanin, 0f, 1f);
            // set.y += 30f;
            // this.compFace.PawnFace.EuMelanin =
            // Widgets.HorizontalSlider(set, this.compFace.PawnFace.EuMelanin, 0f, 1f);
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

            float grey = this.PawnFace.Greyness;
            grey = Widgets.HorizontalSlider(
                set,
                grey,
                HairMelanin.GreyRange.min,
                HairMelanin.GreyRange.max,
                false,
                "FacialStuffEditor.Greyness".Translate(),
                "0",
                "1");

            /*
            Baldness bald = this.compFace.PawnFace.Baldness;
            bald = HorizontalDoubleSlider(
                set,
                bald,
                0,
                10,
                false,
                "FacialStuffEditor.Baldness".Translate(),
                "0",
                "10");
                */
            if (GUI.changed)
            {
                bool update = false;

                if (Math.Abs(this.PawnFace.EuMelanin - euMelanin) > 0.001f)
                {
                    this.PawnFace.EuMelanin = euMelanin;
                    update = true;
                }

                if (Math.Abs(this.PawnFace.Greyness - grey) > 0.001f)
                {
                    this.PawnFace.Greyness = grey;
                    update = true;
                }

                /*
                                if (Math.Abs(this.compFace.PawnFace.Baldness.currentBaldness - bald.currentBaldness) > 0.01f
                                    || Math.Abs(this.compFace.PawnFace.Baldness.maxBaldness - bald.maxBaldness) > 0.01f)
                                {
                                    bald.maxBaldness = Mathf.Max(bald.currentBaldness, bald.maxBaldness);

                                    this.compFace.PawnFace.Baldness = bald;
                                    update = true;
                                }
                                */
                if (update)
                {
                    this.RemoveColorPicker();

                    this.NewHairColor = this.PawnFace.GetCurrentHairColor();
                }
            }
        }

        /*
        // Verse.Widgets
        public static Baldness HorizontalDoubleSlider(Rect rect, Baldness baldness, float leftValue, float rightValue, bool middleAlignment = false, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = 1f)
        {
            if (middleAlignment || !label.NullOrEmpty())
            {
                rect.y += Mathf.Round((rect.height - 16f) / 2f);
            }
            if (!label.NullOrEmpty())
            {
                rect.y += 5f;
            }

            float current = GUI.HorizontalSlider(rect, baldness.currentBaldness, leftValue, rightValue);
            float max = GUI.HorizontalSlider(rect, baldness.maxBaldness, leftValue, rightValue);

            if (!label.NullOrEmpty() || !leftAlignedLabel.NullOrEmpty() || !rightAlignedLabel.NullOrEmpty())
            {
                TextAnchor anchor = Text.Anchor;
                GameFont font = Text.Font;
                Text.Font = GameFont.Tiny;
                float num2 = (!label.NullOrEmpty()) ? Text.CalcSize(label).y : 18f;
                rect.y = rect.y - num2 + 3f;
                if (!leftAlignedLabel.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperLeft;
                    Widgets.Label(rect, leftAlignedLabel);
                }
                if (!rightAlignedLabel.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperRight;
                    Widgets.Label(rect, rightAlignedLabel);
                }
                if (!label.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(rect, label);
                }
                Text.Anchor = anchor;
                Text.Font = font;
            }
          //  if (roundTo > 0f)
            {
                baldness.currentBaldness = (int)(Mathf.RoundToInt(current / roundTo) * roundTo);
                baldness.maxBaldness = (int)(Mathf.RoundToInt(max / roundTo) * roundTo);
            }
            return baldness;
        }
        */
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
                    if (moustache == this._originalMoustache)
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

            GUI.color = Pawn.story.SkinColor;
            GUI.DrawTexture(rect1, Pawn.Drawer.renderer.graphics.headGraphic.MatSouth.mainTexture);
            GUI.DrawTexture(rect2, Pawn.Drawer.renderer.graphics.headGraphic.MatEast.mainTexture);
            GUI.color = this.PawnFace.HasSameBeardColor ? Pawn.story.hairColor : this.NewBeardColor;
            GUI.DrawTexture(rect1, this.MoustacheGraphic(moustache)?.MatSouth.mainTexture);
            GUI.DrawTexture(rect2, this.MoustacheGraphic(moustache)?.MatEast.mainTexture);
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

        // Blatantly stolen from Prepare Carefully
        private void DrawSkinColorSelector(Rect rect)
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
                Color color = PawnSkinColors_FS.SkinColors[i].Color;

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

            Rect melaninSlider = new Rect(
                0,
                swatchRect.yMax + MarginFS / 2,
                contractedBy.width,
                WidgetUtil.SelectionRowHeight);
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
                WidgetUtil.SelectionRowHeight);

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

            if (this.CompFace.Props.hasWrinkles)
            {
                if (Controller.settings.UseWrinkles)
                {
                    Rect wrinkleRect = new Rect(
                        contractedBy.x,
                        detailRect.yMax,
                        contractedBy.width,
                        WidgetUtil.SelectionRowHeight);

                    float wrinkle = this.PawnFace.WrinkleIntensity;

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
                        if (Math.Abs(wrinkle - this.PawnFace.WrinkleIntensity) > 0.001f)
                        {
                            this.PawnFace.WrinkleIntensity = wrinkle;
                            this.RerenderPawn = true;
                        }
                    }
                }
            }

            GUI.EndGroup();
        }
#if legacy

        private void DrawSpecialPicker(Rect rect)
        {
            foreach (GraphicDatabaseHeadRecordsModded.HeadGraphicRecord source in
            GraphicDatabaseHeadRecordsModded.HeadsVanillaCustom)
            {
                source.GetGraphic(Pawn.story.SkinColor);
            }

            // string str = "Naked_" + BodyTypeDefOf.ToString();
            // string path = "Things/Pawn/Humanlike/Bodies/" + str;
            // return GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor);
            List<TabRecord> list = new List<TabRecord>();
            TabRecord item = new TabRecord(
                "HeadType".Translate(),
                delegate
                {
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x =>
                            x.hairTags
                                .SharesElementWith(VanillaHairTags)
                            &&
                            (x.hairGender ==
                             HairGender
                                 .Female ||
                             x.hairGender ==
                             HairGender
                                 .FemaleUsually && !x.IsBeardNotHair()
                            ));
                    HairDefs.SortBy(i => i.LabelCap.ToString());
                    this._specialTab = SpecialTab.Head;
                }, this._specialTab == SpecialTab.Head);
            list.Add(item);

            TabRecord item2 = new TabRecord(
                                            "BodyType".Translate(),
                                            delegate
                                            {
                                                HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                                              x =>
                                                                                                                  x.hairTags
                                                                                                                   .SharesElementWith(VanillaHairTags)
                                                                                                                &&
                                                                                                                  (x.hairGender ==
                                                                                                                   HairGender
                                                                                                                  .Male ||
                                                                                                                   x.hairGender ==
                                                                                                                   HairGender
                                                                                                                  .MaleUsually && !x.IsBeardNotHair()
                                                                                                                  ));
                                                HairDefs.SortBy(i => i.LabelCap.ToString());
                                                this._specialTab = SpecialTab.Body;
                                            }, this._specialTab == SpecialTab.Body);
            list.Add(item2);

            TabRecord item3 = new TabRecord(
                                            "FacialStuffEditor.Any".Translate(),
                                            delegate
                                            {
                                                HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                                              x =>
                                                                                                                  x.hairTags
                                                                                                                   .SharesElementWith(VanillaHairTags) &&
                                                                                                                  x.hairGender ==
                                                                                                                  HairGender
                                                                                                                 .Any && !x.IsBeardNotHair());
                                                HairDefs.SortBy(i => i.LabelCap.ToString());
                                                this.genderTab = GenderTab.Any;
                                            }, this._specialTab == SpecialTab.Head);

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
                    Widgets.BeginScrollView(rect, ref this.ScrollPositionHairMale, rect3);
                    break;

                case GenderTab.Female:
                    Widgets.BeginScrollView(rect, ref this.ScrollPositionHairFemale, rect3);
                    break;

                case GenderTab.Any:
                    Widgets.BeginScrollView(rect, ref this.ScrollPositionHairAny, rect3);
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
#endif
        private Graphic HairGraphic(HairDef def)
        {
            Graphic graphic;
            if (def.texPath != null)
            {
                graphic = GraphicDatabase.Get<Graphic_Multi>(
                                                              def.texPath,
                                                              ShaderDatabase.Cutout,
                                                              new Vector2(38f, 38f),
                                                              Color.white,
                                                              Color.white);
            }
            else
            {
                graphic = null;
            }

            return graphic;
        }

        private Graphic_Multi_NaturalEyes LeftEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes __result;
            if (def != null)
            {
                string path = this.CompFace.EyeTexPath(Side.Left, def);

                __result = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                    path,
                    ShaderDatabase.CutoutComplex,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_Multi_NaturalEyes;
            }
            else
            {
                __result = null;
            }

            return __result;
        }

        private Graphic_Multi_NaturalEars LeftEarGraphic(EarDef def)
        {
            Graphic_Multi_NaturalEars __result;
            if (def != null)
            {
                string path = this.CompFace.EarTexPath(Side.Left, def);

                __result = GraphicDatabase.Get<Graphic_Multi_NaturalEars>(
                    path,
                    ShaderDatabase.CutoutComplex,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_Multi_NaturalEars;
            }
            else
            {
                __result = null;
            }

            return __result;
        }

        [CanBeNull]
        private Graphic_Multi_NaturalHeadParts MoustacheGraphic([NotNull] MoustacheDef def)
        {
            Graphic_Multi_NaturalHeadParts graphic;
            string path = this.CompFace.GetMoustachePath(def);
            if (path.NullOrEmpty())
            {
                graphic = null;
            }
            else
            {

                graphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    path,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_Multi_NaturalHeadParts;

            }
            return graphic;
        }

        private Graphic_Multi_NaturalEyes RightEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes graphic;
            if (def != null)
            {
                string path = this.CompFace.EyeTexPath(Side.Right, def);

                graphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                    path,
                    ShaderDatabase.CutoutComplex,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_Multi_NaturalEyes;
            }
            else
            {
                graphic = null;
            }

            return graphic;
        }

        private Graphic_Multi_NaturalEyes RightEarGraphic(EarDef def)
        {
            Graphic_Multi_NaturalEyes graphic;
            if (def != null)
            {
                string path = this.CompFace.EarTexPath(Side.Right, def);

                graphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                    path,
                    ShaderDatabase.CutoutComplex,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_Multi_NaturalEyes;
            }
            else
            {
                graphic = null;
            }

            return graphic;
        }

        private void UpdatePawnColors(object type, object newValue)
        {
            switch (type)
            {
                case null:
                    return;
                case BeardDef _:
                    this.PawnFace.BeardColor = (Color)newValue;
                    break;
                case HairDef _:
                    Pawn.story.hairColor = (Color)newValue;
                    this.PawnFace.HairColor = (Color)newValue;
                    break;
                case Color _:
                    Pawn.story.melanin = (float)newValue;
                    break;
            }

            // skin color

            this.RerenderPawn = true;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void UpdatePawnDefs([NotNull] Def newValue)
        {
            switch (newValue)
            {
                case BeardDef _:
                    this.PawnFace.BeardDef = (BeardDef)newValue;
                    break;
                case MoustacheDef _:
                    this.PawnFace.MoustacheDef = (MoustacheDef)newValue;
                    break;
                case EyeDef _:
                    this.PawnFace.EyeDef = (EyeDef)newValue;
                    break;
                case EarDef _:
                    this.PawnFace.EarDef = (EarDef)newValue;
                    break;
                case BrowDef _:
                    this.PawnFace.BrowDef = (BrowDef)newValue;
                    break;
                case HairDef _:
                    Pawn.story.hairDef = (HairDef)newValue;
                    break;
            }

            this.RerenderPawn = true;
        }

#endregion Private Methods

        // private void CheckSelectedFacePresetHasName()
        // }
        // }
        // SelectedFacePreset.label = "Unnamed";
        // {
        // if (SelectedFacePreset != null && SelectedFacePreset.label.NullOrEmpty())

        // {
    }

    // public struct Baldness
    // {
    // public int currentBaldness;
    // public int maxBaldness;
    // }
}