using RimWorld;
using UnityEngine;
using Verse;
using static UnityEngine.GUILayout;

namespace RW_FacialStuff
{

    public class FS_Mod : Mod
    {
        #region Fields
        public override string SettingsCategory() => "Facial Stuff";

        private ModSettings modSettings = new FS_Settings();

        #endregion

        public FS_Mod(ModContentPack content) : base(content)
        {
            this.modSettings = this.GetSettings<FS_Settings>();
        }

        #region Methods

        public override void WriteSettings()
        {
            if (this.modSettings != null)
            {
                this.modSettings.Write();
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {

            BeginArea(inRect);
            BeginVertical();
            FS_Settings.UseWrinkles = Toggle(FS_Settings.UseWrinkles, "Settings.UseWrinkles".Translate());
            EndVertical();
            BeginVertical();
            FS_Settings.UseMouth = Toggle(FS_Settings.UseMouth, "Settings.UseMouth".Translate());
            EndVertical();
            BeginVertical();
            FS_Settings.MergeHair = Toggle(FS_Settings.MergeHair, "Settings.MergeHair".Translate());
            EndVertical();
            FlexibleSpace();
            BeginVertical();
            if (Button("Settings.Apply".Translate()))
            {
                foreach (Pawn pawn in PawnsFinder.AllMapsAndWorld_Alive)
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        CompFace faceComp = pawn.TryGetComp<CompFace>();
                        if (faceComp != null)
                        {
                            this.WriteSettings();
                            faceComp.sessionOptimized = false;
                            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                        }

                    }
                }
            }
            EndVertical();
            FlexibleSpace();
            EndArea();
        }

    }
    public class FS_Settings : ModSettings
    {
        public static bool UseMouth = false;

        public static bool UseWrinkles = true;

        public static bool MergeHair = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref UseWrinkles, "UseWrinkles", false, true);
            Scribe_Values.Look(ref UseMouth, "UseMouth", false, true);
            Scribe_Values.Look(ref MergeHair, "MergeHair", false, true);
        }
    }

    #endregion

}

