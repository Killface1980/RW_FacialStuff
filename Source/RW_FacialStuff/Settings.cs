// ReSharper disable StyleCop.SA1401
namespace FacialStuff
{
    using System.Collections.Generic;

    using UnityEngine;

    using Verse;

    public class Settings : ModSettings
    {

        #region Public Fields

        public Vector2 FemaleAverageNormalOffset = new Vector2(0.1761006f, 0.18113f);
        public Vector2 FemaleAveragePointyOffset = new Vector2(0.1937107f, 0.163522f);
        public Vector2 FemaleAverageWideOffset = new Vector2(0.17610f, 0.1861635f);
        public Vector2 FemaleNarrowNormalOffset = new Vector2(0.11824f, 0.2012579f);
        public Vector2 FemaleNarrowPointyOffset = new Vector2(0.13585f, 0.21635f);
        public Vector2 FemaleNarrowWideOffset = new Vector2(0.14088f, 0.191195f);

        public Vector2 MaleAverageNormalOffset = new Vector2(0.2314465f, 0.17862f);
        public Vector2 MaleAveragePointyOffset = new Vector2(0.2339623f, 0.17107f);
        public Vector2 MaleAverageWideOffset = new Vector2(0.23145f, 0.1735849f);
        public Vector2 MaleNarrowNormalOffset = new Vector2(0.13816f, 0.20337f);
        public Vector2 MaleNarrowPointyOffset = new Vector2(0.1534591f, 0.19874f);
        public Vector2 MaleNarrowWideOffset = new Vector2(0.1559749f, 0.2064151f);

        public Vector2 EyeFemaleAverageNormalOffset = new Vector2(-0.01761f, 0f);
        public Vector2 EyeFemaleAveragePointyOffset = new Vector2(-0.00755f, -0.01006f);
        public Vector2 EyeFemaleAverageWideOffset = new Vector2(0.00755f, 0f);
        public Vector2 EyeFemaleNarrowNormalOffset = new Vector2(-0.02264f, 0f);
        public Vector2 EyeFemaleNarrowPointyOffset = new Vector2(-0.02516f, 0f);
        public Vector2 EyeFemaleNarrowWideOffset = new Vector2(-0.01509f, 0f);

        public Vector2 EyeMaleAverageNormalOffset = new Vector2(0f, 0f);
        public Vector2 EyeMaleAveragePointyOffset = new Vector2(-0.01256f, 0f);
        public Vector2 EyeMaleAverageWideOffset = new Vector2(0f, 0f);
        public Vector2 EyeMaleNarrowNormalOffset = new Vector2(-0.02516f, 0f);
        public Vector2 EyeMaleNarrowPointyOffset = new Vector2(-0.02516f, 0f);
        public Vector2 EyeMaleNarrowWideOffset = new Vector2(-0.02516f, 0f);


        #endregion Public Fields

        #region Private Fields

        private bool hideHatInBed = true;

        private bool hideHatWhileRoofed = true;

        private bool makeThemBlink = true;
        private bool mergeHair = true;

        private bool showExtraParts = true;

        private bool useCaching;
        private bool useMouth = true;

        private bool useWeirdHairChoices = true;
        private bool useWrinkles = true;

        private bool showBodyChange;

        private bool showGenderAgeChange;


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

        public bool ShowBodyChange => this.showBodyChange;

        public bool ShowGenderAgeChange => this.showGenderAgeChange;

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
                "FacialStuffEditor.ShowBodyChange".Translate(),
                ref this.showBodyChange);
            list.CheckboxLabeled(
                "FacialStuffEditor.ShowGenderChange".Translate(),
                ref this.showGenderAgeChange);

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
            Scribe_Values.Look(ref this.showBodyChange, "showBodyChange", false, true);
            Scribe_Values.Look(ref this.showGenderAgeChange, "showGenderAgeChange", false, true);
        }

        #endregion Public Methods

    }
}