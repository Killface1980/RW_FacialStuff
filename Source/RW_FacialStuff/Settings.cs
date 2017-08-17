// ReSharper disable StyleCop.SA1401
namespace FacialStuff
{
    using UnityEngine;

    using Verse;

    public class Settings : ModSettings
    {
        #region Public Fields

        public float FemaleAverageNormalOffsetX = 0.1761006f;

        public float FemaleAverageNormalOffsetY = 0.18113f;

        public float FemaleAveragePointyOffsetX = 0.1937107f;

        public float FemaleAveragePointyOffsetY = 0.163522f;

        public float FemaleAverageWideOffsetX = 0.2062893f;

        public float FemaleAverageWideOffsetY = 0.1861635f;

        public float FemaleNarrowNormalOffsetX = 0.1383648f;

        public float FemaleNarrowNormalOffsetY = 0.2012579f;

        public float FemaleNarrowPointyOffsetX = 0.1534591f;

        public float FemaleNarrowPointyOffsetY = 0.21635f;

        public float FemaleNarrowWideOffsetX = 0.1610063f;

        public float FemaleNarrowWideOffsetY = 0.191195f;

        public float MaleAverageNormalOffsetX = 0.2314465f;
        public float MaleAverageNormalOffsetY = 0.17862f;
        public float MaleAveragePointyOffsetX = 0.2339623f;
        public float MaleAveragePointyOffsetY = 0.17107f;
        public float MaleAverageWideOffsetX = 0.2616352f;
        public float MaleAverageWideOffsetY = 0.1735849f;
        public float MaleNarrowNormalOffsetX = 0.163522f;
        public float MaleNarrowNormalOffsetY = 0.20337f;
        public float MaleNarrowPointyOffsetX = 0.1534591f;
        public float MaleNarrowPointyOffsetY = 0.19874f;
        public float MaleNarrowWideOffsetX = 0.1559749f;
        public float MaleNarrowWideOffsetY = 0.2064151f;

        #endregion Public Fields

        #region Private Fields

        private bool hideHatInBed = true;

        private bool hideHatWhileRoofed = true;

        private bool makeThemBlink = true;
        private bool mergeHair = true;

        private bool showExtraParts = true;

        private bool useCaching = false;
        private bool useMouth = true;

        private bool useWeirdHairChoices = true;
        private bool useWrinkles = true;

        #endregion Private Fields

        #region Public Properties

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

        public bool ShowExtraParts => this.showExtraParts;

        public bool UseCaching => this.useCaching;

        public bool UseDNAByFaction { get; } = false;

        public bool UseMouth => this.useMouth;

        public bool UseWeirdHairChoices => this.useWeirdHairChoices;

        public bool UseWrinkles => this.useWrinkles;

        #endregion Public Properties

        #region Public Methods

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard(GameFont.Small) { ColumnWidth = inRect.width / 2 };
            list.Begin(inRect);
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

            // list.CheckboxLabeled(
            //    "Settings.HideHatWhileRoofed".Translate(),
            //    ref this.hideHatWhileRoofed,
            //    "Settings.HideHatWhileRoofedTooltip".Translate());
            // list.CheckboxLabeled(
            //    "Settings.HideHatInBed".Translate(),
            //    ref this.hideHatInBed,
            //    "Settings.HideHatInBedTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.ShowExtraParts".Translate(),
                ref this.showExtraParts,
                "Settings.ShowExtraPartsTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.UseWeirdHairChoices".Translate(),
                ref this.useWeirdHairChoices,
                "Settings.UseWeirdHairChoicesTooltip".Translate());
            list.CheckboxLabeled(
                "Settings.UseCaching".Translate(),
                ref this.useCaching,
                "Settings.UseCachingTooltip".Translate());

            // this.useDNAByFaction = Toggle(this.useDNAByFaction, "Settings.UseDNAByFaction".Translate());
            list.End();

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
            Scribe_Values.Look(ref this.useWrinkles, "useWrinkles", false, true);
            Scribe_Values.Look(ref this.useMouth, "useMouth", false, true);
            Scribe_Values.Look(ref this.mergeHair, "mergeHair", false, true);
            Scribe_Values.Look(ref this.hideHatWhileRoofed, "hideHatWhileRoofed", false, true);
            Scribe_Values.Look(ref this.hideHatInBed, "hideHatInBed", false, true);
            Scribe_Values.Look(ref this.showExtraParts, "showExtraParts", false, true);
            Scribe_Values.Look(ref this.useWeirdHairChoices, "useWeirdHairChoices", false, true);

            // Scribe_Values.Look(ref this.useDNAByFaction, "useDNAByFaction", false, true);
            Scribe_Values.Look(ref this.makeThemBlink, "makeThemBlink", false, true);
            Scribe_Values.Look(ref this.useCaching, "useCaching", false, true);

#if develop // Scribe_Values.Look(ref MaleAverageNormalOffsetX, "MaleAverageNormalOffsetX");

// Scribe_Values.Look(ref MaleAveragePointyOffsetX, "MaleAveragePointyOffsetX");

// Scribe_Values.Look(ref MaleAverageWideOffsetX, "MaleAverageWideOffsetX");

// Scribe_Values.Look(ref FemaleAverageNormalOffsetX, "FemaleAverageNormalOffsetX");

// Scribe_Values.Look(ref FemaleAveragePointyOffsetX, "FemaleAveragePointyOffsetX");

// Scribe_Values.Look(ref FemaleAverageWideOffsetX, "FemaleAverageWideOffsetX");

// Scribe_Values.Look(ref MaleNarrowNormalOffsetX, "MaleNarrowNormalOffsetX");

// Scribe_Values.Look(ref MaleNarrowPointyOffsetX, "MaleNarrowPointyOffsetX");

// Scribe_Values.Look(ref MaleNarrowWideOffsetX, "MaleNarrowWideOffsetX");

// Scribe_Values.Look(ref FemaleNarrowNormalOffsetX, "FemaleNarrowNormalOffsetX");

// Scribe_Values.Look(ref FemaleNarrowPointyOffsetX, "FemaleNarrowPointyOffsetX");

// Scribe_Values.Look(ref FemaleNarrowWideOffsetX, "FemaleNarrowWideOffsetX");

// Scribe_Values.Look(ref MaleAverageNormalOffsetY, "MaleAverageNormalOffsetY");

// Scribe_Values.Look(ref MaleAveragePointyOffsetY, "MaleAveragePointyOffsetY");

// Scribe_Values.Look(ref MaleAverageWideOffsetY, "MaleAverageWideOffsetY");

// Scribe_Values.Look(ref FemaleAverageNormalOffsetY, "FemaleAverageNormalOffsetY");

// Scribe_Values.Look(ref FemaleAveragePointyOffsetY, "FemaleAveragePointyOffsetY");

// Scribe_Values.Look(ref FemaleAverageWideOffsetY, "FemaleAverageWideOffsetY");

// Scribe_Values.Look(ref MaleNarrowNormalOffsetY, "MaleNarrowNormalOffsetY");

// Scribe_Values.Look(ref MaleNarrowPointyOffsetY, "MaleNarrowPointyOffsetY");

// Scribe_Values.Look(ref MaleNarrowWideOffsetY, "MaleNarrowWideOffsetY");

// Scribe_Values.Look(ref FemaleNarrowNormalOffsetY, "FemaleNarrowNormalOffsetY");

// Scribe_Values.Look(ref FemaleNarrowPointyOffsetY, "FemaleNarrowPointyOffsetY");

// Scribe_Values.Look(ref FemaleNarrowWideOffsetY, "FemaleNarrowWideOffsetY");
#endif
        }

        #endregion Public Methods
    }
}