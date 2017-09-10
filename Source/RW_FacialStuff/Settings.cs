// ReSharper disable StyleCop.SA1401
namespace FacialStuff
{
    using System.Collections.Generic;

    using UnityEngine;

    using Verse;

    public class Settings : ModSettings
    {

        #region Public Fields
        public static Vector2 EyeFemaleAverageNormalOffset = new Vector2(-0.01761f, 0f);
        public static Vector2 EyeFemaleAveragePointyOffset = new Vector2(-0.00755f, -0.01006f);
        public static Vector2 EyeFemaleAverageWideOffset = new Vector2(0.00755f, 0f);
        public static Vector2 EyeFemaleNarrowNormalOffset = new Vector2(-0.02264f, 0f);
        public static Vector2 EyeFemaleNarrowPointyOffset = new Vector2(-0.02516f, 0f);
        public static Vector2 EyeFemaleNarrowWideOffset = new Vector2(-0.01509f, 0f);

        public static Vector2 EyeMaleAverageNormalOffset = new Vector2(0f, 0f);
        public static Vector2 EyeMaleAveragePointyOffset = new Vector2(-0.01256f, 0f);
        public static Vector2 EyeMaleAverageWideOffset = new Vector2(0f, 0f);
        public static Vector2 EyeMaleNarrowNormalOffset = new Vector2(-0.02516f, 0f);
        public static Vector2 EyeMaleNarrowPointyOffset = new Vector2(-0.02516f, 0f);
        public static Vector2 EyeMaleNarrowWideOffset = new Vector2(-0.02516f, 0f);

        public static List<Vector2> EyeVector = new List<Vector2>
                                                    {
                                                        EyeMaleAverageNormalOffset,
                                                        EyeMaleAveragePointyOffset,
                                                        EyeMaleAverageWideOffset,
                                                        EyeMaleNarrowNormalOffset,
                                                        EyeMaleNarrowPointyOffset,
                                                        EyeMaleNarrowWideOffset,

                                                        EyeFemaleAverageNormalOffset,
                                                        EyeFemaleAveragePointyOffset,
                                                        EyeFemaleAverageWideOffset,
                                                        EyeFemaleNarrowNormalOffset,
                                                        EyeFemaleNarrowPointyOffset,
                                                        EyeFemaleNarrowWideOffset
                                                    };

        public static Vector2 MouthFemaleAverageNormalOffset = new Vector2(0.14331f, 0.13585f); //
        public static Vector2 MouthFemaleAveragePointyOffset = new Vector2(0.16100f, 0.13333f); //
        public static Vector2 MouthFemaleAverageWideOffset = new Vector2(0.16604f, 0.13962f); //
        public static Vector2 MouthFemaleNarrowNormalOffset = new Vector2(0.12956f, 0.15346f); //
        public static Vector2 MouthFemaleNarrowPointyOffset = new Vector2(0.12328f, 0.16604f); //
        public static Vector2 MouthFemaleNarrowWideOffset = new Vector2(0.12075f, 0.16101f); //

        public static Vector2 MouthMaleAverageNormalOffset = new Vector2(0.18868f, 0.14592f); //
        public static Vector2 MouthMaleAveragePointyOffset = new Vector2(0.19874f, 0.15346f); //
        public static Vector2 MouthMaleAverageWideOffset = new Vector2(0.21636f, 0.14843f); //
        public static Vector2 MouthMaleNarrowNormalOffset = new Vector2(0.12810f, 0.17610f); //
        public static Vector2 MouthMaleNarrowPointyOffset = new Vector2(0.11824f, 0.17358f); //
        public static Vector2 MouthMaleNarrowWideOffset = new Vector2(0.11825f, 0.17623f); //

        public static List<Vector2> MouthVector = new List<Vector2>
                                                    {
                                                        MouthMaleAverageNormalOffset,
                                                        MouthMaleAveragePointyOffset,
                                                        MouthMaleAverageWideOffset,
                                                        MouthMaleNarrowNormalOffset,
                                                        MouthMaleNarrowPointyOffset,
                                                        MouthMaleNarrowWideOffset,

                                                        MouthFemaleAverageNormalOffset,
                                                        MouthFemaleAveragePointyOffset,
                                                        MouthFemaleAverageWideOffset,
                                                        MouthFemaleNarrowNormalOffset,
                                                        MouthFemaleNarrowPointyOffset,
                                                        MouthFemaleNarrowWideOffset
                                                    };

        #endregion Public Fields

        #region Private Fields

        private bool hideHatInBed = true;

        private bool hideHatWhileRoofed = true;

        private bool makeThemBlink = true;
        private bool mergeHair = true;

        private bool showExtraParts = true;

        private bool useCaching;
        private bool useHeadRotator = false;
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

        public bool UseHeadRotator => this.useHeadRotator;
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

            list.CheckboxLabeled(
                "Settings.UseHeadRotator".Translate(),
                ref this.useHeadRotator,
                "Settings.UseHeadRotatorTooltip".Translate());

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
            Scribe_Values.Look(ref this.useHeadRotator, "useHeadRotator", false, true);
            Scribe_Values.Look(ref this.showBodyChange, "showBodyChange", false, true);
            Scribe_Values.Look(ref this.showGenderAgeChange, "showGenderAgeChange", false, true);
        }

        #endregion Public Methods

    }
}