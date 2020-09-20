using PawnPlus.DefOfs;
using PawnPlus.Defs;
using PawnPlus.FaceEditor.ColorPicker;
using PawnPlus.FaceEditor.UI.DTO;
using PawnPlus.FaceEditor.UI.DTO.SelectionWidgetDTOs;
using PawnPlus.FaceEditor.UI.Util;
using PawnPlus.Genetics;
using PawnPlus.Graphics;
using PawnPlus.Harmony;
using PawnPlus.Utilities;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PawnPlus.FaceEditor
{
    [StaticConstructorOnStartup]
    public partial class Dialog_FaceStyling : Window
    {
        private enum BeardTab : byte
        {
            Combinable,
            FullBeards
        }
        
        public enum FaceStyleTab : byte
        {
            Hair,
            Beard,
            Eye,
            Brow,
            TypeSelector
        }
        
        private enum FilterTab : byte
        {
            Urban,
            Rural,
            Punk,
            Tribal
        }
        
        public enum GenderTab : byte
        {
            Female,
            Male,
            Any,
            All
        }
        
        #region Public Fields

        public static readonly Color ColorSwatchBorder = new Color(0.77255f, 0.77255f, 0.77255f);

        public static readonly Color ColorSwatchSelection = new Color(0.9098f, 0.9098f, 0.9098f);

        public static readonly int Columns;

        public static readonly float EntrySize;

        public static readonly float ListWidth = 450f;

        public static readonly float MarginFS = 14f;

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

        public static List<MoustacheDef> MoustacheDefs;
        
        private static readonly List<string> VanillaHairTags = new List<string> { "Urban", "Rural", "Punk", "Tribal" };

        public static List<BrowDef> BrowDefs;

        private static List<string> _currentFilter = new List<string> { "Urban", "Rural", "Punk", "Tribal" };

        private static List<EyeDef> _eyeDefs;

        private static List<HairDef> _hairDefs;

        public static Pawn Pawn;

        private static Vector2 _portraitSize = new Vector2(203f, 203f);

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

        private HairDef _newHair;

        private Color _newHairColor;

        private float _newMelanin;

        private MoustacheDef _newMoustache;

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
        public FaceData PawnFace => this._pawnFace;

#endregion Private Fields

#region Public Constructors
        static Dialog_FaceStyling()
        {
            Columns = 12;
            EntrySize = ListWidth / Columns;
            NameBackground = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));
            HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                                                                                x => x.hairTags
                                                                                      .SharesElementWith(VanillaHairTags) && !x.IsBeardNotHair());

            _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading;
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
            this._pawnFace = this.CompFace.FaceData;

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
        
        private HairDef NewHair
        {
            get => this._newHair;

            set
            {
                this._newHair = value;
                this.UpdatePawnDefs(value);
            }
        }

        private readonly FaceData _pawnFace;

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
                if(RerenderPawn)
                {
                    Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(Pawn);
                    RerenderPawn = false;
                }

                if(!Initialized)
                {
                    DresserDto = new DresserDTO(Pawn);
                    DresserDto.SetUpdatePawnListeners(UpdatePawn);
                }
            }
            catch(Exception ex)
            {
                Log.Error("Facial Stuff: exception in DoWindowContents: " + ex.Message);
            }

            List<TabRecord> tabList = new List<TabRecord>();
            TabRecord hairTab = new TabRecord(
                "FacialStuffEditor.Hair".Translate(),
                delegate { Tab = FaceStyleTab.Hair; }, 
                Tab == FaceStyleTab.Hair);
            tabList.Add(hairTab);
            if(CompFace.Props.hasBeard)
            {
                if(Pawn.gender == Gender.Male)
                {
                    TabRecord beardTab = new TabRecord(
                        "FacialStuffEditor.Beard".Translate(),
                        delegate { Tab = FaceStyleTab.Beard; }, 
                        Tab == FaceStyleTab.Beard);
                    tabList.Add(beardTab);
                }
            }
            if(CompFace.EyeBehavior.NumEyes > 0)
            {
                TabRecord eyeTab = new TabRecord(
                    "FacialStuffEditor.Eye".Translate(),
                    delegate { Tab = FaceStyleTab.Eye; }, 
                    Tab == FaceStyleTab.Eye);
                tabList.Add(eyeTab);

                TabRecord browTab = new TabRecord(
                    "FacialStuffEditor.Brow".Translate(),
                    delegate { Tab = FaceStyleTab.Brow; }, 
                    Tab == FaceStyleTab.Brow);
                tabList.Add(browTab);
            }
            if(Controller.settings.ShowBodyChange && Current.ProgramState == ProgramState.Playing)
            {
                TabRecord typeSelectorTab = new TabRecord(
                    "FacialStuffEditor.TypeSelector".Translate(),
                    delegate { Tab = FaceStyleTab.TypeSelector; }, 
                    Tab == FaceStyleTab.TypeSelector);
                tabList.Add(typeSelectorTab);
            }

            Rect contentRect = new Rect(
                0f,
                TitleHeight + TabDrawer.TabHeight + MarginFS / 2,
                inRect.width,
                inRect.height - TitleHeight - MarginFS * 2 - TabDrawer.TabHeight);

            TabDrawer.DrawTabs(contentRect, tabList);

            DrawUI(contentRect);

            Action backAct = delegate
            {
                RemoveColorPicker();
                if(OriginalGender != Gender.Male && Tab == FaceStyleTab.Beard)
                {
                    Tab = FaceStyleTab.Hair;
                }
                ResetPawnFace();
            };

            DialogUtility.DoNextBackButtons(
                inRect,
                "Randomize".Translate(),
                "FacialStuffEditor.Accept".Translate(),
                backAct, 
                FaceRandomizer, 
                SaveAndClose);
        }

        public void DrawColorSelected(Rect swatchRect)
        {
            Widgets.DrawBoxSolid(swatchRect, Color.white);
        }

        public virtual void DrawUI(Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect pawnRect = AddPortraitWidget(0f, TitleHeight);
            Vector2 pawnNameLblSize = Text.CalcSize(Pawn.LabelShort);
            Rect labelRect = new Rect(0f, pawnRect.yMax, pawnNameLblSize.x, pawnNameLblSize.y);
            labelRect = labelRect.CenteredOnXIn(pawnRect);

            float width = rect.width - ListWidth - MarginFS;

            Rect hairSettingsBtn = new Rect(
                0f,
                labelRect.yMax + MarginFS / 2,
                (width - MarginFS) / 2,
                WidgetUtil.SelectionRowHeight);
            Rect mainRect = new Rect(0f, hairSettingsBtn.yMax + MarginFS, width, 65f);
            if(Widgets.ButtonText(hairSettingsBtn, "FacialStuffEditor.SkinSettings".Translate()))
            {
                RemoveColorPicker();
                SkinPage = true;
            }

            hairSettingsBtn.x = hairSettingsBtn.xMax + MarginFS;

            if(Widgets.ButtonText(hairSettingsBtn, "FacialStuffEditor.HairSettings".Translate()))
            {
                if(Tab == FaceStyleTab.Beard)
                {
                    DoColorWindowBeard();
                }
                SkinPage = false;
            }
            
            Rect listRect = new Rect(
                0f, 
                TitleHeight, 
                ListWidth, 
                rect.height - MarginFS * 3 - TitleHeight) 
            { 
                x = mainRect.xMax + MarginFS 
            };

            mainRect.yMax = listRect.yMax;
            PickerPosition = new Vector2(mainRect.position.x, mainRect.position.y);
            PickerSize = new Vector2(mainRect.width, mainRect.height);

            GUI.DrawTexture(
                new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height),
                NameBackground);
            Widgets.Label(labelRect, Pawn.LabelShort);

            Rect set = 
                new Rect(mainRect)
                { 
                    height = WidgetUtil.SelectionRowHeight, 
                    width = mainRect.width / 2 - 10f 
                };
            set.y = 
                listRect.yMax -
                WidgetUtil.SelectionRowHeight;
            set.width = 
                mainRect.width -
                MarginFS / 3;

            bool faceCompHasSameBeardColor = PawnFace.HasSameBeardColor;

            mainRect.yMax -= WidgetUtil.SelectionRowHeight + MarginFS;
            if(SkinPage)
            {
                DrawSkinColorSelector(mainRect);
            }
            else
            {
                if(!(Tab == FaceStyleTab.Beard && !faceCompHasSameBeardColor))
                {
                    DrawHairColorSelector(mainRect);
                }
                if(Pawn.gender == Gender.Male)
                {
                    Widgets.CheckboxLabeled(
                        set,
                        "FacialStuffEditor.SameColor".Translate(),
                        ref faceCompHasSameBeardColor);
                    TooltipHandler.TipRegion(set, "FacialStuffEditor.SameColorTip".Translate());
                }
            }

            if(Tab == FaceStyleTab.Hair || Tab == FaceStyleTab.Beard)
            {
                listRect.yMin += TabDrawer.TabHeight;
            }

            Widgets.DrawMenuSection(listRect);
            if(GUI.changed)
            {
                if(PawnFace.HasSameBeardColor != faceCompHasSameBeardColor)
                {
                    RemoveColorPicker();
                    PawnFace.HasSameBeardColor = faceCompHasSameBeardColor;
                    NewBeardColor = HairMelanin.ShuffledBeardColor(NewHairColor);
                }
            }

            set.width = mainRect.width / 2 - 10f;

            set.y += 36f;
            set.x = mainRect.x;

			switch(Tab) 
            { 
                case FaceStyleTab.Eye:
                    DrawEyePicker(listRect);
                    break;

                case FaceStyleTab.Brow:
                    if(Pawn.gender == Gender.Female)
                    {
                        BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                            x =>
                                x.hairGender ==
                                HairGender.Female ||
                                x.hairGender ==
                                HairGender.FemaleUsually);
                        BrowDefs.SortBy(i => i.LabelCap.ToString());
                    } else
                    {
                        BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                            x =>
                                x.hairGender == HairGender.Male ||
                                x.hairGender ==
                                HairGender.MaleUsually);
                        BrowDefs.SortBy(i => i.LabelCap.ToString());
                    }
                    DrawBrowPicker(listRect);
                    break;

                case FaceStyleTab.Hair:
                    DrawHairPicker(listRect);
                    break;

                case FaceStyleTab.Beard:
                    DrawBeardPicker(listRect);
                    break;

                case FaceStyleTab.TypeSelector:
                    DrawTypeSelector(listRect);
                    break;
            }
            GUI.EndGroup();
        }

        public void FaceRandomizer()
        {
            while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
            {
            }

            _reInit = true;
            PawnFace.HasSameBeardColor = Rand.Value > 0.3f;
            NewHair = PawnHairChooser.RandomHairDefFor(Pawn, Faction.OfPlayer.def);
            PawnFace.GenerateHairDNA(Pawn, true);
            NewHairColor = PawnFace.HairColor;
            NewBeardColor = PawnFace.BeardColor;
            PawnFaceMaker.RandomBeardDefFor(
                CompFace,
                Faction.OfPlayer.def,
                out BeardDef beard,
                out MoustacheDef tache);
            this.NewBeard = beard;
            this.NewMoustache = tache;

            this.NewEye = PawnFaceMaker.RandomEyeDefFor(
                Pawn,
                Faction.OfPlayer.def,
                CompFace.Props);
            this.NewBrow = PawnFaceMaker.RandomBrowDefFor(Pawn, Faction.OfPlayer.def);

            this._reInit = false;
        }

        public override void PostClose()
        {
            base.PostClose();
            Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public override void PostOpen()
        {
            FillDefs();
            _hairDefs.SortBy(i => i.label);
            _eyeDefs.SortBy(i => i.label);
            BrowDefs.SortBy(i => i.label);
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
            this.Close();
        }
        
        public void UpdatePawn(object sender, object value, object value2 = null)
        {
            if(sender == null)
            {
                return;
            }

            if(sender is BodyTypeSelectionDto)
            {
                Pawn.story.bodyType = (BodyTypeDef)value;
            }
            else if(sender is GenderSelectionDto)
            {
                Pawn.gender = (Gender)value;
            }
            else if(sender is HeadTypeSelectionDto)
            {
                typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(Pawn.story, value);
                if(value2 != null)
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
                        x => x.hairTags.SharesElementWith(VanillaHairTags) && 
                            (x.hairGender == HairGender.Male || 
                            x.hairGender == HairGender.MaleUsually && 
                            !x.IsBeardNotHair()));
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    break;

                case Gender.Female:
                    HairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairTags.SharesElementWith(VanillaHairTags) && 
                            (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually && !x.IsBeardNotHair()));
                    _eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    BrowDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    break;
            }
        }

        [CanBeNull]
        private Graphic_Multi BeardGraphic([NotNull] BeardDef def)
        {
            string path = CompFace.PawnFaceGraphic.GetBeardPath(def);

            Graphic_Multi graphic =
            GraphicDatabase.Get<Graphic_Multi>(
                path,
                ShaderDatabase.Cutout,
                new Vector2(38f, 38f),
                Color.white,
                Color.white) as Graphic_Multi;

            return graphic;
        }

        private Graphic_Multi BrowGraphic(BrowDef def)
        {
            Graphic_Multi __result =
            GraphicDatabase.Get<Graphic_Multi>(
                CompFace.PawnFaceGraphic.BrowTexPath(def),
                ShaderDatabase.CutoutSkin,
                new Vector2(38f, 38f),
                Color.white,
                Color.white) as Graphic_Multi;
            
            return __result;
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

            if(GUI.changed)
            {
                bool update = false;

                if(Math.Abs(this.PawnFace.EuMelanin - euMelanin) > 0.001f)
                {
                    this.PawnFace.EuMelanin = euMelanin;
                    update = true;
                }

                if(Math.Abs(this.PawnFace.Greyness - grey) > 0.001f)
                {
                    this.PawnFace.Greyness = grey;
                    update = true;
                }
                if(update)
                {
                    this.RemoveColorPicker();

                    this.NewHairColor = this.PawnFace.GetCurrentHairColor();
                }
            }
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

        private Graphic_FacePart LeftEyeGraphic(EyeDef def)
        {
            Graphic_FacePart __result;
            if (def != null)
            {
                string path = CompFace.PawnFaceGraphic.EyeTexturePath(def);

                __result = GraphicDatabase.Get<Graphic_FacePart>(
                    path,
                    ShaderDatabase.CutoutComplex,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_FacePart;
            }
            else
            {
                __result = null;
            }

            return __result;
        }
        
        [CanBeNull]
        private Graphic_Multi MoustacheGraphic([NotNull] MoustacheDef def)
        {
            Graphic_Multi graphic;
            string path = CompFace.PawnFaceGraphic.GetMoustachePath(def);
            if (path.NullOrEmpty())
            {
                graphic = null;
            }
            else
            {
                graphic = GraphicDatabase.Get<Graphic_Multi>(
                    path,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_Multi;
            }
            return graphic;
        }

        private Graphic_FacePart RightEyeGraphic(EyeDef def)
        {
            Graphic_FacePart graphic;
            if (def != null)
            {
                string path = CompFace.PawnFaceGraphic.EyeTexturePath(def);

                graphic = GraphicDatabase.Get<Graphic_FacePart>(
                    path,
                    ShaderDatabase.CutoutComplex,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white) as Graphic_FacePart;
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

        private void UpdatePawnDefs(Def newValue)
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
    }
}
