// ReSharper disable StyleCop.SA1401

namespace FacialStuff
{
    using System.Collections.Generic;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class Settings : ModSettings
    {
        private bool filterHats = true;

        private bool hideHatInBed = true;

        private bool hideHatWhileRoofed = false;

        private bool makeThemBlink = true;

        private bool mergeHair = true;

        private bool showBodyChange;

        private bool showExtraParts = true;

        private bool showGenderAgeChange;

        private bool useCaching;

        private bool useFreeWill;

        private bool useNastyGrin = false;

        private bool useHeadRotator = true;

        private bool useMouth = true;

        private bool useWeirdHairChoices = true;

        private bool useWrinkles = true;

        public bool HideShellWhileRoofed
        {
            get
            {
                return hideShellWhileRoofed;
            }
        }

        private bool hideShellWhileRoofed = true;

        private bool sameBeardColor;

        public bool SameBeardColor => sameBeardColor;

        public bool FilterHats
        {
            get => this.filterHats;
            set
            {
                this.filterHats = value;
                PortraitsCache.Clear();
            }
        }

        public bool HideHatInBed
        {
            get => this.hideHatInBed;
            set => this.hideHatInBed = value;
        }

        public bool HideHatWhileRoofed
        {
            get => this.hideHatWhileRoofed;
            set => this.hideHatWhileRoofed = value;
        }

        public bool MakeThemBlink => this.makeThemBlink;

        public bool MergeHair => this.mergeHair;

        public bool ShowBodyChange => this.showBodyChange;

        public bool ShowExtraParts => this.showExtraParts;

        public bool ShowGenderAgeChange => this.showGenderAgeChange;

        public bool UseCaching => this.useCaching;

        public bool UseNastyGrin => this.useNastyGrin;

        public bool UseDNAByFaction { get; } = false;

        public bool UseHeadRotator => this.useHeadRotator;

        public bool UseFreeWill => this.useFreeWill;

        public bool UseMouth => this.useMouth;

        public bool UseWeirdHairChoices => this.useWeirdHairChoices;

        public bool UseWrinkles => this.useWrinkles;

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard(GameFont.Small) { ColumnWidth = inRect.width / 2 };
            list.Begin(inRect);

            list.Gap();
            list.Label("Settings.VisibilityHeadgearLabel".Translate());
            list.GapLine();

            list.CheckboxLabeled(
                "Settings.HideHatWhileRoofed".Translate(),
                ref this.hideHatWhileRoofed,
                "Settings.HideHatWhileRoofedTooltip".Translate());

            list.CheckboxLabeled(
                "Settings.FilterHats".Translate(),
                ref this.filterHats,
                "Settings.FilterHatsTooltip".Translate());

            list.CheckboxLabeled(
                "Settings.HideHatInBed".Translate(),
                ref this.hideHatInBed,
                "Settings.HideHatInBedTooltip".Translate());

            list.CheckboxLabeled(
                "Settings.HideShellWhileRoofed".Translate(),
                ref this.hideShellWhileRoofed,
                "Settings.HideShellWhileRoofedTooltip".Translate());

            list.Gap();

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





            // list.CheckboxLabeled(
            //     "Settings.UseFreeWill".Translate(),
            //     ref this.useFreeWill,
            //     "Settings.UseFreeWillTooltip".Translate());

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
            // pawn.Drawer.renderer.graphics.ResolveAllGraphics();
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
        }
    }
}