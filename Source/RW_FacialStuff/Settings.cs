// ReSharper disable StyleCop.SA1401

namespace FacialStuff
{
    using System.Collections.Generic;

    using FacialStuff.Enums;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class Settings : ModSettings
    {
        #region Private Fields

        private bool filterHats = true;

        private bool hideHatInBed = true;

        private bool hideHatWhileRoofed = false;

        private bool hideShellWhileRoofed = true;
        private bool ignoreRenderBody;
        private bool ignoreWhileDrafted;
        private MaxLayerToShow layerInBed = MaxLayerToShow.OnSkin;
        private MaxLayerToShow layerInOwnedBed = MaxLayerToShow.Naked;
        private MaxLayerToShow layerInPrivateRoom = MaxLayerToShow.OnSkin;
        private MaxLayerToShow layerInRoom = MaxLayerToShow.Middle;
        private bool makeThemBlink = true;

        private bool mergeHair = true;

        private bool sameBeardColor;
        private bool showBodyChange;

        private bool showExtraParts = true;

        private bool showGenderAgeChange;

        private bool useCaching;

        private bool useFreeWill;

        private bool useHeadRotator = true;
        private bool useMouth = true;
        private bool useNastyGrin = false;
        private bool useWeirdHairChoices = true;

        private bool useWrinkles = true;

        #endregion Private Fields

        #region Public Properties

        public bool FilterHats
        {
            get => this.filterHats;
            set
            {
                this.filterHats = value;
                PortraitsCache.Clear();
            }
        }

        public bool HideHatInBed => this.hideHatInBed;

        public bool HideHatWhileRoofed => this.hideHatWhileRoofed;

        public bool HideShellWhileRoofed
        {
            get
            {
                return hideShellWhileRoofed;
            }
        }
        public bool IgnoreRenderBody => this.ignoreRenderBody;
        public bool IgnoreWhileDrafted => this.ignoreWhileDrafted;
        public MaxLayerToShow LayerInBed => layerInBed;
        public MaxLayerToShow LayerInOwnedBed => this.layerInOwnedBed;
        public MaxLayerToShow LayerInPrivateRoom => this.layerInPrivateRoom;
        public MaxLayerToShow LayerInRoom => this.layerInRoom;
        public bool MakeThemBlink => this.makeThemBlink;
        public bool MergeHair => this.mergeHair;
        public bool SameBeardColor => sameBeardColor;
        public bool ShowBodyChange => this.showBodyChange;

        public bool ShowExtraParts => this.showExtraParts;

        public bool ShowGenderAgeChange => this.showGenderAgeChange;

        public bool UseCaching => this.useCaching;

        public bool UseDNAByFaction { get; } = false;
        public bool UseFreeWill => this.useFreeWill;
        public bool UseHeadRotator => this.useHeadRotator;
        public bool UseMouth => this.useMouth;
        public bool UseNastyGrin => this.useNastyGrin;
        public bool UseWeirdHairChoices => this.useWeirdHairChoices;

        public bool UseWrinkles => this.useWrinkles;

        #endregion Public Properties

        #region Public Methods

        public void DoWindowContents(Rect inRect)
        {
            Rect rect = inRect.ContractedBy(15f);
            Listing_Standard list = new Listing_Standard(GameFont.Small) { ColumnWidth = (rect.width / 2) - 17f };

            list.Begin(rect);

            list.Label("Settings.PawnFeaturesLabel".Translate());
            list.GapLine();

            list.CheckboxLabeled(
                "Settings.UseHeadRotator".Translate(),
                ref this.useHeadRotator,
                "Settings.UseHeadRotatorTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.MakeThemBlink".Translate(),
                ref this.makeThemBlink,
                "Settings.MakeThemBlinkTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.UseMouth".Translate(),
                ref this.useMouth,
                "Settings.UseMouthTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.UseWrinkles".Translate(),
                ref this.useWrinkles,
                "Settings.UseWrinklesTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.MergeHair".Translate(),
                ref this.mergeHair,
                "Settings.MergeHairTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.SameBeardColor".Translate(),
                ref this.sameBeardColor,
                "Settings.SameBeardColorTooltip".Translate());

            // list.CheckboxLabeled(
            // "Settings.HideHatWhileRoofed".Translate(),
            // ref this.hideHatWhileRoofed,
            // "Settings.HideHatWhileRoofedTooltip".Translate());
            // list.CheckboxLabeled(
            // "Settings.HideHatInBed".Translate(),
            // ref this.hideHatInBed,
            // "Settings.HideHatInBedTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.ShowExtraParts".Translate(),
                ref this.showExtraParts,
                "Settings.ShowExtraPartsTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.UseWeirdHairChoices".Translate(),
                ref this.useWeirdHairChoices,
                "Settings.UseWeirdHairChoicesTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.UseNastyGrin".Translate(),
                ref this.useNastyGrin,
                "Settings.UseNastyGrinTooltip".Translate());

            list.Gap();
            list.Label("Settings.EditorLabel".Translate());
            list.GapLine();
            list.CheckboxLabeled("FacialStuffEditor.ShowBodyChange".Translate(), ref this.showBodyChange);
            list.CheckboxLabeled("FacialStuffEditor.ShowGenderChange".Translate(), ref this.showGenderAgeChange);

            list.Gap();
            list.Label("Settings.ExperimentalLabel".Translate());
            list.GapLine();
            list.CheckboxLabeled(
                "Settings.UseCaching".Translate(),
                ref this.useCaching,
                "Settings.UseCachingTooltip".Translate());

            list.NewColumn();

            list.Label("Settings.VisibilityHeadgearLabel".Translate());
            list.GapLine();



            list.Gap();
            list.Label("Hats");

            list.CheckboxLabeled(
                "Settings.HideHatWhileRoofed".Translate(),
                ref this.hideHatWhileRoofed,
                "Settings.HideHatWhileRoofedTooltip".Translate());

            if (!this.HideHatWhileRoofed)
            {
                list.CheckboxLabeled(
                    "Settings.HideHatInBed".Translate(),
                    ref this.hideHatInBed,
                    "Settings.HideHatInBedTooltip".Translate());
            }

            list.CheckboxLabeled(
                "Settings.FilterHats".Translate(),
                ref this.filterHats,
                "Settings.FilterHatsTooltip".Translate());


            list.Gap();
            list.Label("Body");

            list.CheckboxLabeled(
                "Settings.HideShellWhileRoofed".Translate(),
                ref this.hideShellWhileRoofed,
                "Settings.HideShellWhileRoofedTooltip".Translate());

            if (this.HideShellWhileRoofed)
            {
                list.Label("Apparel layers to show:");
                list.LabelDouble("In a room: ", this.LayerInRoom.ToString());
                this.layerInRoom = (MaxLayerToShow)list.Slider((float)this.layerInRoom, 0, 2);

                list.LabelDouble("In private room: ", this.LayerInPrivateRoom.ToString());
                this.layerInPrivateRoom = (MaxLayerToShow)list.Slider((float)this.layerInPrivateRoom, 0, 2);
            }

            list.Gap();
            list.Label("Visibility Of Bodies");
            list.GapLine();

            list.CheckboxLabeled(
                "Settings.IgnoreRenderBody".Translate(),
                ref this.ignoreRenderBody,
                "Settings.IgnoreRenderBodyTooltip".Translate());

            if (this.IgnoreRenderBody)
            {
                if (this.HideShellWhileRoofed)
                {
                    list.LabelDouble("Apparel layers in bed: ", this.LayerInBed.ToString());
                    this.layerInBed = (MaxLayerToShow)list.Slider((float)this.layerInBed, 0, 2);
                    list.LabelDouble("Apparel layers in owned bed: ", this.LayerInOwnedBed.ToString());
                    this.layerInOwnedBed = (MaxLayerToShow)list.Slider((float)this.layerInOwnedBed, 0, 2);
                }
            }

            if (this.HideHatWhileRoofed || this.HideShellWhileRoofed)
            {
                list.GapLine(24f);
                list.CheckboxLabeled(
                    "Settings.IgnoreWhileDrafted".Translate(),
                    ref this.ignoreWhileDrafted,
                    "Settings.IgnoreWhileDraftedTooltip".Translate());
            }

            // if (list.ButtonText("Reset"))
            // {
            // Controller.settings = new Settings();
            // }


            // list.CheckboxLabeled(
            // "Settings.UseFreeWill".Translate(),
            // ref this.useFreeWill,
            // "Settings.UseFreeWillTooltip".Translate());

            // this.useDNAByFaction = Toggle(this.useDNAByFaction, "Settings.UseDNAByFaction".Translate());
            list.End();

            if (GUI.changed)
            {
                this.Mod.WriteSettings();
            }

            // FlexibleSpace();
            // BeginVertical();
            // if (Button("Settings.Apply".Translate()))
            // {
            // foreach (Pawn pawn in PawnsFinder.AllMapsAndWorld_Alive)
            // {
            // if (pawn.RaceProps.Humanlike)
            // {
            // CompFace faceComp = pawn.TryGetComp<CompFace>();
            // if (faceComp != null)
            // {
            // this.WriteSettings();
            // faceComp.sessionOptimized = false;
            // pawn.FaceDrawer.renderer.graphics.ResolveAllGraphics();
            // }
            // }
            // }
            // }
            // EndVertical();
            // FlexibleSpace();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.hideShellWhileRoofed, "hideShellWhileRoofed", false, true);
            Scribe_Values.Look(ref this.useWrinkles, "useWrinkles", false, true);
            Scribe_Values.Look(ref this.useMouth, "useMouth", false, true);
            Scribe_Values.Look(ref this.mergeHair, "mergeHair", false, true);
            Scribe_Values.Look(ref this.hideHatWhileRoofed, "hideHatWhileRoofed", false, true);
            Scribe_Values.Look(ref this.hideHatInBed, "hideHatInBed", false, true);
            Scribe_Values.Look(ref this.showExtraParts, "showExtraParts", false, true);
            Scribe_Values.Look(ref this.useWeirdHairChoices, "useWeirdHairChoices", false, true);
            Scribe_Values.Look(ref this.filterHats, "filterHats", false, true);
            Scribe_Values.Look(ref this.sameBeardColor, "sameBeardColor", false, true);

            // Scribe_Values.Look(ref this.useDNAByFaction, "useDNAByFaction", false, true);
            Scribe_Values.Look(ref this.makeThemBlink, "makeThemBlink", false, true);
            Scribe_Values.Look(ref this.useCaching, "useCaching", false, true);
            Scribe_Values.Look(ref this.useNastyGrin, "useNastyGrin", false, true);
            Scribe_Values.Look(ref this.useHeadRotator, "useHeadRotator", false, true);
            Scribe_Values.Look(ref this.showBodyChange, "showBodyChange", false, true);
            Scribe_Values.Look(ref this.showGenderAgeChange, "showGenderAgeChange", false, true);

            Scribe_Values.Look(ref this.ignoreWhileDrafted, "ignoreWhileDrafted", false, true);
            Scribe_Values.Look(ref this.ignoreRenderBody, "ignoreRenderBody", false, true);

            Scribe_Values.Look(ref this.layerInRoom, "layerInRoom");
            Scribe_Values.Look(ref this.layerInPrivateRoom, "layerInPrivateRoom");
            Scribe_Values.Look(ref this.layerInBed, "layerInBed");
            Scribe_Values.Look(ref this.layerInOwnedBed, "layerInOwnedBed");
        }

        #endregion Public Methods
    }
}