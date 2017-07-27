using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff
{
    using UnityEngine;
    using static UnityEngine.GUILayout;

    using Verse;

    public class Settings : ModSettings
    {
        private  bool useMouth = true;

        private bool useWrinkles = true;

        private bool mergeHair = true;

        private bool hideHatInBed = true;

        private bool showExtraParts = true;

        private bool useHairDNA = true;

        private bool makeThemBlink = true;

        public bool UseMouth => this.useMouth;

        public bool UseWrinkles => this.useWrinkles;

        public bool MergeHair => this.mergeHair;

        public bool HideHatInBed => this.hideHatInBed;

        public bool ShowExtraParts => this.showExtraParts;

        public bool UseHairDNA => this.useHairDNA;

        public bool MakeThemBlink => this.makeThemBlink;

        public bool UseDNAByFaction => this.useDNAByFaction;

        public float MaleAverageNormalOffsetX = 0.2314465f;

        public  float MaleAveragePointyOffsetX = 0.2339623f;

        public  float MaleAverageWideOffsetX = 0.2616352f;

        public  float FemaleAverageNormalOffsetX = 0.1761006f;

        public  float FemaleAveragePointyOffsetX = 0.1937107f;

        public  float FemaleAverageWideOffsetX = 0.2062893f;

        public  float MaleNarrowNormalOffsetX = 0.163522f;

        public  float MaleNarrowPointyOffsetX = 0.1534591f;

        public  float MaleNarrowWideOffsetX = 0.1559749f;

        public  float FemaleNarrowNormalOffsetX = 0.1383648f;

        public  float FemaleNarrowPointyOffsetX = 0.1534591f;

        public  float FemaleNarrowWideOffsetX = 0.1610063f;

        public  float MaleAverageNormalOffsetY = 0.17862f;

        public  float MaleAveragePointyOffsetY = 0.17107f;

        public  float MaleAverageWideOffsetY = 0.1735849f;

        public  float FemaleAverageNormalOffsetY = 0.18113f;

        public  float FemaleAveragePointyOffsetY = 0.163522f;

        public  float FemaleAverageWideOffsetY = 0.1861635f;

        public  float MaleNarrowNormalOffsetY = 0.20337f;

        public  float MaleNarrowPointyOffsetY = 0.19874f;

        public  float MaleNarrowWideOffsetY = 0.2064151f;

        public  float FemaleNarrowNormalOffsetY = 0.2012579f;

        public  float FemaleNarrowPointyOffsetY = 0.21635f;

        public  float FemaleNarrowWideOffsetY = 0.191195f;

        private bool useDNAByFaction = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.useWrinkles, "useWrinkles", false, true);
            Scribe_Values.Look(ref this.useMouth, "useMouth", false, true);
            Scribe_Values.Look(ref this.mergeHair, "mergeHair", false, true);
            Scribe_Values.Look(ref this.hideHatInBed, "hideHatInBed", false, true);
            Scribe_Values.Look(ref this.showExtraParts, "showExtraParts", false, true);
            Scribe_Values.Look(ref this.useHairDNA, "useHairDNA", false, true);
            Scribe_Values.Look(ref this.useDNAByFaction, "useDNAByFaction", false, true);
            Scribe_Values.Look(ref this.makeThemBlink, "makeThemBlink", false, true);

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

        public void DoWindowContents(Rect inRect)
        {
            BeginArea(inRect);
            BeginVertical();
            this.useWrinkles = Toggle(this.useWrinkles, "Settings.UseWrinkles".Translate());

            this.useMouth = Toggle(this.useMouth, "Settings.UseMouth".Translate());

            this.mergeHair = Toggle(this.mergeHair, "Settings.MergeHair".Translate());

            this.hideHatInBed = Toggle(this.hideHatInBed, "Settings.HideHatInBed".Translate());

            this.showExtraParts = Toggle(this.showExtraParts, "Settings.ShowExtraParts".Translate());

            this.useDNAByFaction = Toggle(this.useDNAByFaction, "Settings.UseDNAByFaction".Translate());

            this.useHairDNA = Toggle(this.useHairDNA, "Settings.UseHairDNA".Translate());

            this.makeThemBlink = Toggle(this.makeThemBlink, "Settings.MakeThemBlink".Translate());
            EndVertical();

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
            EndArea();
        }
    }

}
